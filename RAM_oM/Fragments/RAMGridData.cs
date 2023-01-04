/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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


using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.RAM
{
    public class RAMGridData : IFragment
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Represents Grid System label as per RAM")]
        public virtual string Label { get; set; } = null;

        [Description("Represents Grid System type as per RAM")]
        public virtual string Type { get; set; } = null;

        [Description("Represents Grid System X offset")]
        public virtual double XOffset { get; set; } = 0;

        [Description("Represents Grid System Y offset")]
        public virtual double YOffset { get; set; } = 0;

        [Description("Represents Grid System rotation")]
        public virtual double Rotation { get; set; } = 0;

        /***************************************************/
    }
}


