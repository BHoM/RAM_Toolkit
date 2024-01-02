/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.SettingOut;
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

        private List<Panel> ReadPanels(List<string> ids = null)
        {
            //Get dictionary of surface properties with ids
            Dictionary<string, ISurfaceProperty> bhomProperties = ReadISurfaceProperties().ToDictionary(x => GetAdapterId(x).ToString());

            //Implement code for reading panels
            List<Panel> bhomPanels = new List<Panel>();

            //Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {

                //Get Walls
                IWalls IWalls = IStories.GetAt(i).GetWalls();
                int numWalls = IWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall IWall = IWalls.GetAt(j);
                    Panel Panel = BH.Adapter.RAM.Convert.ToBHoMObject(IWall);
                    bhomPanels.Add(Panel);
                }

                //Get Floors
                IStory IStory = IStories.GetAt(i);
                IFloorType IFloorType = IStory.GetFloorType();
                IDecks IDecks = IFloorType.GetDecks();
                int IStoryUID = IStory.lUID;

                int numDecks = IDecks.GetCount();

                if (numDecks > 0)
                {
                    // Convert Floors
                    for (int j = 0; j < numDecks; j++)
                    {
                        IDeck IDeck = IDecks.GetAt(j);

                        Panel panel = BH.Adapter.RAM.Convert.ToBHoMObject(IDeck, m_Model, IStoryUID);

                        if (panel != null)
                        {
                            ISurfaceProperty bhProp = new ConstantThickness();
                            bhomProperties.TryGetValue(IDeck.lPropID.ToString(), out bhProp);

                            if (bhProp != null) panel.Property = bhProp;
                            else Engine.Base.Compute.RecordWarning($"Could not get property for floor with RAM lUID = {IDeck.lUID}");

                            bhomPanels.Add(panel);
                        }
                    }
                }
                else
                {
                    BH.Engine.Base.Compute.RecordWarning("This story has no slab edges defined. IStoryUID: " + IStoryUID);
                    break;
                }
            }

            return bhomPanels;
        }

        /***************************************************/

        private List<IWall> ReadRamWalls(IModel ramModel)
        {
            //Get stories
            IStories ramStories = ramModel.GetStories();
            int numStories = ramStories.GetCount();
            List<IWall> allRamWalls = new List<IWall>();

            // Get all walls on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Walls
                IWalls ramWalls = ramStories.GetAt(i).GetWalls();
                int numWalls = ramWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall ramWall = ramWalls.GetAt(j);
                    allRamWalls.Add(ramWall);
                }
            }

            return allRamWalls;
        }

        /***************************************************/

    }

}





