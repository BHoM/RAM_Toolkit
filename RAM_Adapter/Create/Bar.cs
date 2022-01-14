/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Spatial.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Constraints;
using BH.oM.Structure.MaterialFragments;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.Engine.Base;
using BH.Engine.Structure;
using BH.Engine.Spatial;
using BH.Adapter.RAM;
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Structure.Loads;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<Bar> bhomBars)
        {

            //Code for creating a collection of bars in the software
            List<Bar> bars = bhomBars.ToList();

            //Get the stories in the model
            IStories ramStories = m_Model.GetStories();

            //Cycle through bars, split to beam and col lists, then add to corresponding story.
            List<Bar> barBeams = new List<Bar>();
            List<Bar> barCols = new List<Bar>();

            foreach (Bar testBar in bars)
            {
                bool isBeam = Math.Abs(testBar.Tangent(true).DotProduct(Vector.ZAxis)) < 0.5;

                if (isBeam) { barBeams.Add(testBar); }
                else { barCols.Add(testBar); }
            }

            //Create beams per story, flat
            foreach (Bar bar in barBeams)
            {
                string name = bar.Name;
                ILayoutBeam ramBeam;

                try
                {
                    RAMId RAMId = new RAMId();

                    IStory barStory = bar.GetStory(StructuralUsage1D.Beam, ramStories);

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutBeams ramBeams = ramFloorType.GetLayoutBeams();

                    double zStart = bar.StartNode.Position().Z.ToInch() - barStory.dElevation;
                    double zEnd = bar.EndNode.Position().Z.ToInch() - barStory.dElevation;

                    //  Get beam fragment cantilever data
                    double startCant = 0;
                    double endCant = 0;
                    bool isStubCant = false;
                    RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        startCant = ramFrameData.StartCantilever;
                        endCant = ramFrameData.EndCantilever;
                        isStubCant = ramFrameData.IsStubCantilever;
                    }

                    if (isStubCant.Equals("True") || isStubCant.Equals("1")) //Check bool per RAM or GH preferred boolean context
                    {
                        SCoordinate startPt, endPt;
                        if (startCant > 0) // Ensure startPt corresponds with support point
                        {
                            startPt = bar.EndNode.Position().ToRAM();
                            endPt = bar.StartNode.Position().ToRAM();
                        }
                        else
                        {
                            startPt = bar.StartNode.Position().ToRAM();
                            endPt = bar.EndNode.Position().ToRAM();
                        }

                        ramBeam = ramBeams.AddStubCantilever(bar.SectionProperty.Material.ToRAM(), startPt.dXLoc, startPt.dYLoc, 0, endPt.dXLoc, endPt.dYLoc, 0); // No Z offsets, beams flat on closest story
                    }
                    else
                    {
                        //  Get support points
                        Vector barDir = bar.Tangent(true);
                        Point startSupPt = BH.Engine.Geometry.Modify.Translate(bar.StartNode.Position(), barDir * startCant);
                        Point endSupPt = BH.Engine.Geometry.Modify.Translate(bar.EndNode.Position(), -barDir * endCant);
                        SCoordinate start = startSupPt.ToRAM();
                        SCoordinate end = endSupPt.ToRAM();

                        ramBeam = ramBeams.Add(bar.SectionProperty.Material.ToRAM(), start.dXLoc, start.dYLoc, 0, end.dXLoc, end.dYLoc, 0); // No Z offsets, beams flat on closest story
                        if (startSupPt.X < endSupPt.X || (startSupPt.X == endSupPt.X && startSupPt.Y > endSupPt.Y))
                        {
                            ramBeam.dStartCantilever = startCant.FromInch();
                            ramBeam.dEndCantilever = endCant.FromInch();
                        }
                        else
                        {
                            ramBeam.dStartCantilever = endCant.FromInch();
                            ramBeam.dEndCantilever = startCant.FromInch();
                        }
                    }

                    // Add warning to report distance of snapping to level as required for RAM
                    if (zStart != 0 || zEnd != 0)
                    { Engine.Base.Compute.RecordWarning("Bar " + name + " snapped to level " + barStory.strLabel + ". Bar moved " + Math.Round(zStart, 2).ToString() + " inches at start and " + Math.Round(zEnd, 2).ToString() + " inches at end."); }

                    IBeams beamsOnStory = barStory.GetBeams();
                    IBeam beam = beamsOnStory.Get(ramBeam.lUID);
                    beam.strSectionLabel = bar.SectionProperty.Name;
                    // beam.EAnalyzeFlag = EAnalyzeFlag.eAnalyze; deprecated in API 
                    RAMId.Id = beam.lUID;
                    bar.SetAdapterId(RAMId);
                }
                catch
                {
                    CreateElementError("bar", name);
                }
            }

            //Create columns at each story with offset per actual height
            foreach (Bar bar in barCols)
            {
                string name = bar.Name;

                try
                {
                    RAMId RAMId = new RAMId();

                    IStory barStory = bar.GetStory(StructuralUsage1D.Column, ramStories);

                    List<Node> colNodes = new List<Node>() { bar.StartNode, bar.EndNode };
                    colNodes.Sort((x, y) => x.Position.Z.CompareTo(y.Position.Z));

                    double xBtm = colNodes[0].Position.X.ToInch();
                    double yBtm = colNodes[0].Position.Y.ToInch();
                    double zBtm = colNodes[0].Position.Z.ToInch() - barStory.dElevation;
                    double xTop = colNodes[1].Position.X.ToInch();
                    double yTop = colNodes[1].Position.Y.ToInch();
                    double zTop = colNodes[1].Position.Z.ToInch() - barStory.dElevation + barStory.dFlrHeight;

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutColumns ramColumns = ramFloorType.GetLayoutColumns();
                    ILayoutColumn ramColumn;

                    //  Get RAM column data
                    bool isHanging = false;
                    RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        isHanging = ramFrameData.IsHangingColumn;
                    }

                    if (isHanging.Equals("True") || isHanging.Equals("1")) //Check bool per RAM or GH preferred boolean context
                    {
                        ramColumn = ramColumns.Add3(bar.SectionProperty.Material.ToRAM(), xBtm, yBtm, xTop, yTop, 0, 0, 1); //No Z offsets, cols start and end at stories
                    }
                    else if (bar.IsVertical())
                    {
                        //Failing if no section property is provided
                        ramColumn = ramColumns.Add(bar.SectionProperty.Material.ToRAM(), xTop, yTop, 0, 0); //No Z offsets, cols start and end at stories
                    }
                    else
                    {
                        ramColumn = ramColumns.Add2(bar.SectionProperty.Material.ToRAM(), xTop, yTop, xBtm, yBtm, 0, 0); //No Z offsets, cols start and end at stories
                    }

                    //Set column properties
                    IColumns colsOnStory = barStory.GetColumns();
                    IColumn column = colsOnStory.Get(ramColumn.lUID);
                    column.strSectionLabel = bar.SectionProperty.Name;
                    column.EAnalyzeFlag = EAnalyzeFlag.eAnalyze;
                    RAMId.Id = column.lUID;
                    bar.SetAdapterId(RAMId);
                }
                catch
                {
                    CreateElementError("bar", name);
                }
            }

            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }


        /***************************************************/
    }
}

