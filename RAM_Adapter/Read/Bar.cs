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
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Results;
using BH.oM.Structure.Loads;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.Requests;
using BH.oM.Analytical.Results;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.Engine.Base;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private List<Bar> ReadBars(List<string> ids = null)
        {
            //Implement code for reading bars
            List<Bar> bhomBars = new List<Bar>();

            // Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Columns
                IColumns IColumns = IStories.GetAt(i).GetColumns();
                int numColumns = IColumns.GetCount();

                //Get Beams
                IFloorType IFloorType = IStories.GetAt(i).GetFloorType();
                ILayoutBeams ILayoutBeams = IFloorType.GetLayoutBeams();
                IBeams IBeams = IStories.GetAt(i).GetBeams();
                int numLayoutBeams = ILayoutBeams.GetCount();
                int numBeams = IBeams.GetCount();

                //Get Vertical Braces
                IVerticalBraces IVBraces = IStories.GetAt(i).GetVerticalBraces();
                int numVBraces = IVBraces.GetCount();

                //Get Horizontal Braces
                ILayoutHorizBraces ILayoutHorizBraces = IStories.GetAt(i).GetFloorType().GetLayoutHorizBraces();
                IHorizBraces IHorizBraces = IStories.GetAt(i).GetHorizBraces();
                int numHBraces = ILayoutHorizBraces.GetCount();

                //Get Elevation
                double dElevation = IStories.GetAt(i).dElevation;

                // Convert Columns
                for (int j = 0; j < numColumns; j++)
                {
                    IColumn IColumn = IColumns.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IColumn);
                    RAMFrameData ramFrameData = bhomBar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        ramFrameData.FloorType = IFloorType.strLabel;
                        bhomBar.Fragments.AddOrReplace(ramFrameData);
                    }
                    bhomBars.Add(bhomBar);
                }

                // Convert Beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam IBeam = IBeams.GetAt(j);
                    ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IBeam, ILayoutBeam, dElevation);
                    RAMFrameData ramFrameData = bhomBar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        ramFrameData.FloorType = IFloorType.strLabel;
                        bhomBar.Fragments.AddOrReplace(ramFrameData);
                    }
                    bhomBars.Add(bhomBar);
                }

                // Convert Vertical Braces
                for (int j = 0; j < numVBraces; j++)
                {
                    IVerticalBrace IVerticalBrace = IVBraces.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IVerticalBrace);
                    RAMFrameData ramFrameData = bhomBar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        ramFrameData.FloorType = IFloorType.strLabel;
                        bhomBar.Fragments.AddOrReplace(ramFrameData);
                    }
                    bhomBars.Add(bhomBar);
                }

                // Convert Horizontal Braces
                for (int j = 0; j < numHBraces; j++)
                {
                    IHorizBrace IHorizBrace = IHorizBraces.GetAt(j);
                    ILayoutHorizBrace ILayoutHorizBrace = ILayoutHorizBraces.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IHorizBrace, ILayoutHorizBrace, dElevation);
                    RAMFrameData ramFrameData = bhomBar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null)
                    {
                        ramFrameData.FloorType = IFloorType.strLabel;
                        bhomBar.Fragments.AddOrReplace(ramFrameData);
                    }
                    bhomBars.Add(bhomBar);
                }
            }

            return bhomBars;

        }

        /***************************************************/

        private List<IBeam> ReadRamBeams(IModel ramModel)
        {
            //Get stories
            IStories ramStories = ramModel.GetStories();
            int numStories = ramStories.GetCount();
            List<IBeam> allRamBeams = new List<IBeam>();

            // Get all beams on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get beams
                IBeams ramBeams = ramStories.GetAt(i).GetBeams();
                int numBeams = ramBeams.GetCount();

                // Convert beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam ramBeam = ramBeams.GetAt(j);
                    allRamBeams.Add(ramBeam);
                }
            }

            return allRamBeams;
        }

        /***************************************************/

    }

}



