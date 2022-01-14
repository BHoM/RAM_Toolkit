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

        private bool CreateCollection(IEnumerable<ContourLoadSet> loads)
        {
            foreach (ContourLoadSet load in loads)
            {
                try
                {
                    //Ensure points describe a closed polyline
                    List<Point> loadPoints = load.Contour.ControlPoints();
                    if (loadPoints.First() != loadPoints.Last())
                    {
                        loadPoints.Add(loadPoints.Last().DeepClone());
                    }

                    //Find the layout to apply to
                    IStories ramStories = m_Model.GetStories();
                    IStory loadStory = loadPoints.First().GetStory(ramStories);
                    double storyElev = loadStory.dElevation;
                    IFloorType floorType = loadStory.GetFloorType();

                    ISurfaceLoadSets floorLoads = floorType.GetSurfaceLoadSets2();
                    int nextId = floorLoads.GetCount();
                    ISurfaceLoadSet ramLoad = floorLoads.Add(nextId, loadPoints.Count());
                    IPoints verticePoints = ramLoad.GetPoints();

                    List<SCoordinate> checkList = new List<SCoordinate>();
                    SCoordinate verticeCoord;

                    for (int i = 0; i < loadPoints.Count(); i++)
                    {
                        verticeCoord = loadPoints[i].ToRAM();
                        verticePoints.Delete(i);
                        verticePoints.InsertAt2(i, verticeCoord.dXLoc, verticeCoord.dYLoc, 0);
                        checkList.Add(verticeCoord);
                    }
                    ramLoad.SetPoints(verticePoints);

                    ramLoad.lPropertySetUID = (int)GetAdapterId(load.UniformLoadSet);
                }

                catch
                {
                    CreateElementError("UniformLoadSet", load.Name);
                }
            }



            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }

        /***************************************************/
    }
}

