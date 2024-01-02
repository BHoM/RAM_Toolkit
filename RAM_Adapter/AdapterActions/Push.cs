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
using BH.oM.Spatial.SettingOut;
using BH.oM.Base;
using BH.oM.Adapter;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {

        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            //Push levels before all other objects.
            List<object> sortedObjects = objects.Where(x => x is Level).ToList();
            sortedObjects.AddRange(objects.Where(x => !(x is Level)));

            List<object> result = new List<object>();

            if (OpenDatabase())
            {
                //Call base push
                try
                {
                    result = base.Push(sortedObjects, tag, pushType, actionConfig);
                }
                finally
                {
                    CloseDatabase();
                }
            }

            return result;
        }

        /***************************************************/
    }
}





