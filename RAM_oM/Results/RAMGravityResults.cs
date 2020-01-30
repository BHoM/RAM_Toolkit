/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Structure.Results;
using BH.oM.Geometry;

namespace BH.oM.Adapters.RAM
{
    public class RAMPointGravityLoad : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public int ObjectId { get; set; } = 0;
        public double dist { get; set; } = 0;
        public double DL { get; set; } = 0;
        public double RedLL { get; set; } = 0;
        public double NonRLL { get; set; } = 0;
        public double StorLL { get; set; } = 0;
        public double RoofLL { get; set; } = 0;
        public string type { get; set; } = "";

        /***************************************************/
    }

    /***************************************************/

    public class RAMLineGravityLoad : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public int ObjectId { get; set; } = 0;
        public double dist1 { get; set; } = 0;
        public double dist2 { get; set; } = 0;
        public double DL1 { get; set; } = 0;
        public double DL2 { get; set; } = 0;
        public double LL1 { get; set; } = 0;
        public double LL2 { get; set; } = 0;
        public double PL1 { get; set; } = 0;
        public double PL2 { get; set; } = 0;
        public string type { get; set; } = "";

        /***************************************************/
    }

    /***************************************************/

    public class RAMFactoredEndReactions : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public int ObjectId { get; set; } = 0;
        public NodeReaction StartReaction { get; set; } = null;
        public NodeReaction EndReaction { get; set; } = null;

        /***************************************************/
    }

    /***************************************************/
}
