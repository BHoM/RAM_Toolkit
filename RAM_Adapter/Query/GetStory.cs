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
using System.Collections.Generic;
using System.Linq;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.oM.Adapters.RAM;
using BH.oM.Structure.MaterialFragments;
using BH.Engine.Structure;
using BH.Engine.Base;
using BH.Engine.Geometry;
using RAMDATAACCESSLib;
using BH.Engine.Units;


namespace BH.Adapter.RAM
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IStory GetStory(this Bar bar, StructuralUsage1D usage1D, IStories ramStories)
        {
            List<Point> barEnds = new List<Point> { bar.StartNode.Position, bar.EndNode.Position };

            switch (usage1D)
            {                  
                case StructuralUsage1D.Column:
                    //  Get RAM column data
                    RAMFrameData ramFrameData = bar.FindFragment<RAMFrameData>(typeof(RAMFrameData));
                    if (ramFrameData != null && ramFrameData.IsHangingColumn)
                    {
                        return barEnds.OrderBy(pt => pt.Z).FirstOrDefault().GetStory(ramStories);
                    }
                    else  //Column to be placed on the level it supports.
                    {
                        return barEnds.OrderByDescending(pt => pt.Z).FirstOrDefault().GetStory(ramStories);
                    }
                case StructuralUsage1D.Beam:
                default:
                    //Use lowest end elevation
                    return barEnds.OrderBy(pt => pt.Z).FirstOrDefault().GetStory(ramStories);
            }
        }

        /***************************************************/

        public static IStory GetStory(this Node node, IStories ramStories)
        {
            return node.Position.GetStory(ramStories);
        }

        /***************************************************/

        public static IStory GetStory(this Point point, IStories ramStories)
        {

            double elev = point.ToRAM().dZLoc;

            //There must be a better way to iterate over IStories
            List <IStory> storeys = new List<IStory>();
            int numStories = ramStories.GetCount();
            for (int i = 0; i < numStories; i++)
            {
                storeys.Add(ramStories.GetAt(i));
            }
            return storeys.OrderBy(x => Math.Abs(x.dElevation - elev)).First();
        }

        /***************************************************/

        public static IStory GetStory(this Panel panel, IStories ramStories)
        {            
            // Get heights of wall and floor corners to create levels
            PolyCurve panelOutline = Engine.Spatial.Query.OutlineCurve(panel);
            List<Point> panelPoints = panelOutline.DiscontinuityPoints();

            Point highPoint = panelPoints.OrderByDescending(pt => pt.Z).FirstOrDefault();

            return highPoint.GetStory(ramStories);
        }

        /***************************************************/

    }

}


