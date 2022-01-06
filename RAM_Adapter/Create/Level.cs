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
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<Level> bhomLevels)
        {
            if (bhomLevels.Count() != 0)
            {
                //sort levels by elevation
                IOrderedEnumerable<Level> orderedBhomLevels = bhomLevels.OrderBy(o => o.Elevation);
                List<Level> sortedBhomLevels = new List<Level>();

                //Check levels for negatives
                if (orderedBhomLevels.First().Elevation < 0)
                {
                    throw new Exception("Base level can not be negative for RAM. Please move model origin point to set all geometry and levels at 0 or greater.");
                }

                //Check levels for base level = 0, remove if occurs
                if (orderedBhomLevels.First().Elevation == 0)
                {
                    sortedBhomLevels = orderedBhomLevels.Where(level => level.Elevation != 0).ToList();
                }
                else
                {
                    sortedBhomLevels = orderedBhomLevels.Where(level => level.Elevation != 0).ToList();
                }

                // Register Floor types
                IFloorTypes ramFloorTypes;
                IFloorType ramFloorType = null;
                IStories ramStories;

                //Create floor type at each level
                for (int i = 0; i < sortedBhomLevels.Count(); i++)
                {
                    Level level = sortedBhomLevels.ElementAt(i);
                    double levelHtDbl = level.Elevation.ToInch();
                    double levelHt = Math.Round(levelHtDbl, 3);

                    // Get elevations and skip if level elevation already in RAM
                    ramStories = m_Model.GetStories();
                    List<double> ramElevs = new List<double>();
                    List<string> ramStoryNames = new List<string>();
                    for (int j = 0; j < ramStories.GetCount(); j++)
                    {
                        ramElevs.Add(ramStories.GetAt(j).dElevation);
                        ramStoryNames.Add(ramStories.GetAt(j).strLabel);
                    }

                    if (ramElevs.Contains(levelHt) != true && ramStoryNames.Contains(level.Name) != true)
                    {
                        double height;
                        // Ground floor ht = 0 for RAM
                        if (i == 0)
                        {
                            height = levelHt;
                        }
                        else
                        {
                            Level lastLevel = sortedBhomLevels.ElementAt(i - 1);
                            height = levelHt - lastLevel.Elevation.ToInch();
                        }

                        int newIndex;
                        if (ramElevs.FindIndex(x => x > levelHt) == -1)
                        {
                            newIndex = ramElevs.Count();
                        }
                        else
                        {
                            newIndex = ramElevs.FindIndex(x => x > levelHt);
                        }

                        List<string> ramFloorTypeNames = new List<string>();
                        ramFloorTypes = m_Model.GetFloorTypes();
                        Boolean floorTypeExists = false;
                        for (int j = 0; j < ramFloorTypes.GetCount(); j++)
                        {
                            IFloorType testFloorType = ramFloorTypes.GetAt(j);
                            if (testFloorType.strLabel == level.Name)
                            {
                                ramFloorType = testFloorType;
                                floorTypeExists = true;
                            }
                        }

                        if (floorTypeExists == false)
                        {
                            ramFloorType = ramFloorTypes.Add(level.Name);
                        }

                        // Modify story above if not top floor
                        if (newIndex < ramStories.GetCount())
                        {
                            IStory ramStoryAbove = ramStories.GetAt(newIndex);
                            ramStoryAbove.dFlrHeight = ramStoryAbove.dElevation - levelHt;
                        }
                        if (newIndex > 0 && ramStories.GetCount() > 0)
                        {
                            IStory ramStoryBelow = ramStories.GetAt(newIndex - 1);
                            height = levelHt - ramStoryBelow.dElevation;
                        }

                        // Insert story at index
                        ramStories.InsertAt(newIndex, ramFloorType.lUID, level.Name, height);
                    }
                }

                //Save file
                m_IDBIO.SaveDatabase();

            }
            return true;

        }

        /***************************************************/
    }
}

