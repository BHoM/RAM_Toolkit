/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Structure.MaterialFragments;
using BH.Engine.Structure;
using BH.Engine.Geometry;
using RAMDATAACCESSLib;


namespace BH.Engine.RAM
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        //Add methods for converting From BHoM to the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static RAMNode ToRAM(this Node node)
        //{
        //    //Insert code for convertion
        //}
        /***************************************************/

        public static ILayoutBeam ToRAM(this Bar bar, ILayoutBeams ILayoutBeams)
        {
            ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(0);

            double xStart = bar.StartNode.Position().X;
            double yStart = bar.StartNode.Position().Y;
            double xEnd = bar.EndNode.Position().X;
            double yEnd = bar.EndNode.Position().Y;

            //Set support coordinates and name
            //CAUTION: different from actual end points and cantilevers hardcoded
            ILayoutBeam.SetLayoutCoordinates(xStart, yStart, 0, xEnd, yEnd, 0, 0, 0);
            ILayoutBeam.strSectionLabel = bar.SectionProperty.Name;

            return ILayoutBeam;
        }

        /***************************************************/

        public static SCoordinate ToRAM(this Point point)
        {
            SCoordinate Point = new SCoordinate();
            Point.dXLoc = point.X;
            Point.dYLoc = point.Y;
            Point.dZLoc = point.Z;
            return Point;
        }

        /***************************************************/

        public static EMATERIALTYPES ToRAM(this IMaterialFragment material)
        {
            EMATERIALTYPES Material = new EMATERIALTYPES();
            
            if (material.IMaterialType() == MaterialType.Concrete) { Material = EMATERIALTYPES.EConcreteMat; }
            else if (material.IMaterialType() == MaterialType.Steel) { Material = EMATERIALTYPES.ESteelMat; }
            else { Material = EMATERIALTYPES.ESteelMat; }
            return Material;
        }

        /***************************************************/

        public static IStory GetStory(this Bar bar, StructuralUsage1D usage1D, IStories ramStories)
        {
            double elev;
            switch (usage1D)
            {
                case StructuralUsage1D.Beam:
                    //Use lowest end elevation
                    elev = Math.Min(bar.StartNode.Position().Z, bar.EndNode.Position().Z);
                    break;
                case StructuralUsage1D.Column:
                    object isHanging;
                    bar.CustomData.TryGetValue("IsHangingColumn", out isHanging);
                    isHanging = isHanging == null ? "" : isHanging.ToString();

                    if (isHanging == "True" || isHanging == "1") //Hanging Column to be placed on its btm level.
                    {
                        elev = Math.Min(bar.StartNode.Position().Z, bar.EndNode.Position().Z);
                    }
                    else  //Column to be placed on the level it supports.
                    {
                        elev = Math.Max(bar.StartNode.Position().Z, bar.EndNode.Position().Z);
                    }
                    break;
                default:
                    //Use lowest end elevation
                    elev = Math.Min(bar.StartNode.Position().Z, bar.EndNode.Position().Z);
                    break;
            }

            //There must be a better way to iterate over IStories
            List<IStory> storeys = new List<IStory>();
            int numStories = ramStories.GetCount();
            for (int i = 0; i < numStories; i++)
            {
                storeys.Add(ramStories.GetAt(i));
            }
            return storeys.OrderBy(x => Math.Abs(x.dElevation - elev)).First();
        }

        /***************************************************/

        public static IStory GetStory(this Node node, IStories ramStories)
        {
            double elev = node.Position().Z;

            //There must be a better way to iterate over IStories
            List<IStory> storeys = new List<IStory>();
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
            double elev;

            List<double> panelHeights = new List<double>();
            List<Point> panelPoints = new List<Point>();
            
            // Get heights of wall and floor corners to create levels
            PolyCurve panelOutline = Engine.Structure.Query.Outline(panel);
            panelPoints = panelOutline.DiscontinuityPoints();

            foreach (Point pt in panelPoints)
            {
                panelHeights.Add(Math.Round(pt.Z, 0));
            }

            // Get elevation of panel per max elevation
            elev = panelHeights.Max();

            //There must be a better way to iterate over IStories
            List<IStory> storeys = new List<IStory>();
            int numStories = ramStories.GetCount();
            for (int i = 0; i < numStories; i++)
            {
                storeys.Add(ramStories.GetAt(i));
            }
            return storeys.OrderBy(x => Math.Abs(x.dElevation - elev)).First();
        }

        /***************************************************/

}

}
