/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Geometry.SettingOut;
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

        private bool CreateCollection(IEnumerable<Bar> bhomBars)
        {

            //Code for creating a collection of bars in the software
            List<Bar> bars = bhomBars.ToList();

            //Get the stories in the model
            IStories ramStories = m_Model.GetStories();

            //Cycle through bars, split to beam and col lists, then add to corresponding story.
            List<Bar> barBeams = bhomBars.Where(bar => !bar.IsColumn(ramStories)).ToList();
            List<Bar> barCols  = bhomBars.Where(bar => bar.IsColumn(ramStories)).ToList();

            //Create beams per story, flat
            CreateBeams(ramStories, barBeams);

            //Create columns at each story with offset per actual height
            CreateColumns(ramStories, barCols);

            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }

        /***************************************************/

        private void CreateBeams(IStories ramStories, List<Bar> barBeams)
        {
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


                    RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));

                    if (ramFrameData != null && ramFrameData.IsStubCantilever)
                    {
                        SCoordinate start = bar.StartNode.Position().ToRAM();
                        SCoordinate end = bar.EndNode.Position().ToRAM();

                        if (ramFrameData.StartCantilever > 0) // Ensure startPt corresponds with support point                            
                        {
                            ramBeam = ramBeams.AddStubCantilever(bar.SectionProperty.Material.ToRAM(), end.dXLoc, end.dYLoc, 0, start.dXLoc, start.dYLoc, 0); // No Z offsets, beams flat on closest story
                        }
                        else
                        {
                            ramBeam = ramBeams.AddStubCantilever(bar.SectionProperty.Material.ToRAM(), start.dXLoc, start.dYLoc, 0, end.dXLoc, end.dYLoc, 0); // No Z offsets, beams flat on closest story
                        }
                    }
                    else
                    {
                        //  Get beam fragment cantilever data
                        double startCant = 0;
                        double endCant = 0;

                        Bar trimBar = TrimCantilevers(bar, out startCant, out endCant);

                        SCoordinate start = trimBar.StartNode.Position.ToRAM();
                        SCoordinate end = trimBar.EndNode.Position.ToRAM();

                        ramBeam = ramBeams.Add(trimBar.SectionProperty.Material.ToRAM(), start.dXLoc, start.dYLoc, start.dZLoc - barStory.dElevation, end.dXLoc, end.dYLoc, end.dZLoc - barStory.dElevation); // No Z offsets, beams flat on closest story

                        ramBeam.dStartCantilever = startCant.ToInch();
                        ramBeam.dEndCantilever = endCant.ToInch();
                    }

                    // Add warning to report distance of snapping to level as required for RAM
                    double zStart = bar.StartNode.Position().ToRAM().dZLoc - barStory.dElevation;
                    double zEnd = bar.EndNode.Position().ToRAM().dZLoc - barStory.dElevation;

                    if (zStart != 0 || zEnd != 0)
                    { Engine.Reflection.Compute.RecordWarning("Bar " + name + " snapped to level " + barStory.strLabel + ". Bar moved " + Math.Round(zStart, 2).ToString() + " inches at start and " + Math.Round(zEnd, 2).ToString() + " inches at end."); }

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
        }

        /***************************************************/

        private void CreateColumns(IStories ramStories, List<Bar> barCols)
        {
            //Create columns at each story with offset per actual height
            foreach (Bar bar in barCols)
            {
                string name = bar.Name;

                try
                {
                    RAMId RAMId = new RAMId();

                    IStory barStory = bar.GetStory(StructuralUsage1D.Column, ramStories);

                    List<Node> colNodes = new List<Node>() { bar.StartNode, bar.EndNode }.OrderBy( node => node.Position.Z ).ToList();

                    SCoordinate btm = colNodes[0].Position.ToRAM();
                    SCoordinate top = colNodes[1].Position.ToRAM();

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutColumns ramColumns = ramFloorType.GetLayoutColumns();
                    ILayoutColumn ramColumn;

                    //  Get RAM column data
                    RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));

                    if (ramFrameData != null && ramFrameData.IsHangingColumn) //Check bool per RAM or GH preferred boolean context
                    {
                        ramColumn = ramColumns.Add3(bar.SectionProperty.Material.ToRAM(), btm.dXLoc, btm.dYLoc, top.dXLoc, top.dYLoc, 0, 0, 1); //No Z offsets, cols start and end at stories
                    }
                    else if (bar.IsVertical())
                    {
                        //Failing if no section property is provided
                        ramColumn = ramColumns.Add(bar.SectionProperty.Material.ToRAM(), top.dXLoc, top.dYLoc, 0, 0); //No Z offsets, cols start and end at stories
                    }
                    else
                    {
                        ramColumn = ramColumns.Add2(bar.SectionProperty.Material.ToRAM(), top.dXLoc, top.dYLoc, btm.dXLoc, btm.dYLoc, 0, 0); //No Z offsets, cols start and end at stories
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
        }

        /***************************************************/

        private static Bar TrimCantilevers(Bar bar, out double startCant, out double endCant)
        {
            //  Get beam fragment cantilever data
            startCant = 0;
            endCant = 0;

            RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));

            if (ramFrameData != null)
            {
                startCant = ramFrameData.StartCantilever;
                endCant = ramFrameData.EndCantilever;
            }

            //  Get support points
            Vector barDir = bar.Tangent(true);

            Bar trimBar = bar.DeepClone();
            trimBar.StartNode.Position = BH.Engine.Geometry.Modify.Translate(bar.StartNode.Position(), barDir * startCant);
            trimBar.EndNode.Position = BH.Engine.Geometry.Modify.Translate(bar.EndNode.Position(), -barDir * endCant);

            if (barDir.DotProduct(Vector.XAxis) < 0 || barDir.DotProduct(Vector.YAxis) == 1)
            {
                double temp = endCant;
                endCant = startCant;
                startCant = temp;

                return trimBar.Flip();
            }

            return trimBar;
        }

        private bool CreateCollection(IEnumerable<ISectionProperty> sectionProperties)
        {
            //Code for creating a collection of section properties in the software

            //Not yet implemented

            return true;
        }

        /***************************************************/

    }
}