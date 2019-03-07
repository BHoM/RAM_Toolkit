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
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.oM.Common;
using BH.oM.Structure.Loads;
using BH.Engine.Structure;
using BH.oM.Structure.Properties;
using RAMDATAACCESSLib;
using BH.oM.Architecture.Elements;


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


        //public static IBeam ToRAM(Bar bar)
        //{
        //    IBeam IBeam;
        //    return IBeam;
        //}

        public static ILayoutBeam ToRAM(Bar bar, ILayoutBeams ILayoutBeams)
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

        public static SCoordinate ToRAM(Point point)
        {
            SCoordinate Point = new SCoordinate();
            Point.dXLoc = point.X;
            Point.dYLoc = point.Y;
            Point.dZLoc = point.Z;
            return Point;
        }

        public static EMATERIALTYPES ToRAM(oM.Common.Materials.Material material)
        {
            EMATERIALTYPES Material = new EMATERIALTYPES();
            
            if (material.Type == oM.Common.Materials.MaterialType.Concrete) { Material = EMATERIALTYPES.EConcreteMat; }
            else if (material.Type == oM.Common.Materials.MaterialType.Steel) { Material = EMATERIALTYPES.ESteelMat; }
            else { Material = EMATERIALTYPES.ESteelMat; }
            return Material;
        }

        public static string ToRAM(string BHoMSectionName)
        {
            string RAMSecName = BHoMSectionName;
            return RAMSecName;
        }

        /*
        public static EGridAxis ToRAM(ICurve curve)
        {
            ICurve gridCrv = curve;
            // get the orintation of the curve
        
            EGridAxis crvAxis = (0,1,0);  
            return crvAxis; 
        }

        */

           
    }

}
