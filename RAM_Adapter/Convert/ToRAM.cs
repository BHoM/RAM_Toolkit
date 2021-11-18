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
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ILayoutBeam ToRAM(this Bar bar, ILayoutBeams iLayoutBeams)
        {
            ILayoutBeam iLayoutBeam = iLayoutBeams.GetAt(0);

            SCoordinate start = bar.StartNode.Position().ToRAM();
            SCoordinate end = bar.EndNode.Position().ToRAM();

            //Set support coordinates and name
            //CAUTION: different from actual end points and cantilevers hardcoded
            iLayoutBeam.SetLayoutCoordinates(start.dXLoc, start.dYLoc, 0, end.dXLoc, end.dYLoc, 0, 0, 0);
            iLayoutBeam.strSectionLabel = bar.SectionProperty.DescriptionOrName();
            return iLayoutBeam;
        }

        /***************************************************/

        public static SCoordinate ToRAM(this Point point)
        {
            SCoordinate pt = new SCoordinate();
            pt.dXLoc = point.X.ToInch();
            pt.dYLoc = point.Y.ToInch();
            pt.dZLoc = point.Z.ToInch();
            return pt;
        }

        /***************************************************/

        public static EMATERIALTYPES ToRAM(this IMaterialFragment bhMaterial)
        {
            switch (bhMaterial.IMaterialType())
            {
                case MaterialType.Concrete:
                    return EMATERIALTYPES.EConcreteMat;
                case MaterialType.Steel:
                    return EMATERIALTYPES.ESteelMat;
                default:
                    return EMATERIALTYPES.ESteelMat;
            }
        }

        /***************************************************/

    }

}


