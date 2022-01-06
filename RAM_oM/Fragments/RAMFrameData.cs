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


using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.RAM
{
    public class RAMFrameData : IFragment
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Represents Frame Material as per RAM")]
        public virtual string Material { get; set; } = null;

        [Description("Represents Frame Type as per RAM")]
        public virtual string FrameType { get; set; } = null;

        [Description("Represents Floor Type element is placed on as per RAM")]
        public virtual string FloorType { get; set; } = null;

        [Description("Represents Frame Number per RAM")]
        public virtual int FrameNumber { get; set; } = -1;

        [Description("Represents if the element is a stub cantilever in RAM")]
        public virtual bool IsStubCantilever{ get; set; } = false;

        [Description("Represents the start cantilever distance (if applicable) in RAM")]
        public virtual double StartCantilever { get; set; } = 0;

        [Description("Represents the end cantilever distance (if applicable) in RAM")]
        public virtual double EndCantilever { get; set; } = 0;

        [Description("Represents if the element is a hanging column in RAM")]
        public virtual bool IsHangingColumn { get; set; } = false;

        [Description("Number of studs applied to frame")]
        public virtual int Studs { get; set; } = 0;

        [Description("Camber length applied to frame")]
        public virtual double Camber { get; set; } = 0;

        [Description("Design Capacity Interaction as per RAM")]
        public virtual double DesignCapacityInteraction { get; set; } = double.NaN;

        [Description("Critical Deflection Interaction as per RAM")]
        public virtual double CriticalDeflectionInteraction { get; set; } = double.NaN;

        /***************************************************/
    }
}

