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
    public class RAMNodeForceData : IFragment
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Member Force axial at node per RAM")]
        public virtual double Axial { get; set; } = double.NaN;

        [Description("Node distance location along member per RAM")]
        public virtual double Location { get; set; } = double.NaN;

        [Description("Member Force moment maximum at node per RAM")]
        public virtual double MomMaj { get; set; } = double.NaN;

        [Description("Member Force moment minimum at node per RAM")]
        public virtual double MomMin { get; set; } = double.NaN;

        [Description("Member Force shear maximum at node per RAM")]
        public virtual double ShearMaj { get; set; } = double.NaN;

        [Description("Member Force shear minimum at node per RAM")]
        public virtual double ShearMin { get; set; } = double.NaN;

        [Description("Member Force Torsion at node per RAM")]
        public virtual double Torsion { get; set; } = double.NaN;

        [Description("Represents Loadcase ID as per RAM")]
        public virtual string LoadcaseID { get; set; } = null;

        /***************************************************/
    }
}


