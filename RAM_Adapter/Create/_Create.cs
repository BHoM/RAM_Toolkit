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
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            bool success = true;        //Boolean returning if the creation was successful or not

            // Create objects per type
            if (objects.Count() > 0)
            {

                success = CreateCollection(objects as dynamic);

            }
            return success;             //Finally return if the creation was successful or not

        }

        /***************************************************/
    }
}



