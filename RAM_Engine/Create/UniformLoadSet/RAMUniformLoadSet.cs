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
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Loads;
using BH.oM.Adapters.RAM;
using RAMDATAACCESSLib;
using BH.Engine.Reflection;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;


namespace BH.Engine.Adapters.RAM
{
    public static partial class Create
    {
        [Description("Creates a UniformLoadSet specifically for use in RAM. RAM has built-in loadcases, so other loadcases must be mapped to them.")]
        [Input("sdl", "post-composite dead load which is applied to cured concrete decks, i.e. (assuming self weight of slab is set to be counted in the Self-Weight Criteria menu).")]
        [Input("cdl", "pre-composite dead load including any allowance for ponding (assuming self weight of slab is set to be counted in the Self-Weight Criteria menu).")]
        [Input("liveLoad","loads to be applied post composite such as occupancy loads. These should generally be entered without reduction.")]
        [Input("llType","sets a flag for which type of live load has been entered, which impacts how the program will apply reduction or combine with snow loading.")]
        [Input("partition","additional live loads such as partitions which will not be reduced, regardless of the value of llType.")]
        [Input("cll", "live loads from construction activities which will be present during the pre-composite stage.")]
        [Input("massDl", "dead loads which contribute to lateral mass of the building, does not include self weight of modelled elements provided Self-Weight is accounted for in the Criteria. Generally this value will be similar to the SDL value")]
        [Input("name", "A useful and descriptive name which will be used in RAM to refer to this set of loads.")]
        [Output("a set of area loads suitable for pushing to RAM.")]
        public static UniformLoadSet RAMUniformLoadSet(double sdl, double cdl, double liveLoad, RAMLiveLoadTypes llType, double partition, double cll, double massDl, string name = "")
        {

            UniformLoadSet loadSet = new UniformLoadSet
            {
                Name = name,
                Loads = new List<UniformLoadSetRecord>
                {
                    new UniformLoadSetRecord(){ Name = ELoadCaseType.DeadLCa.ToString(), Load = sdl },
                    new UniformLoadSetRecord(){ Name = ELoadCaseType.ConstructionDeadLCa.ToString(), Load = cdl },
                    new UniformLoadSetRecord(){ Name = ELoadCaseType.PartitionLCa.ToString(), Load = partition },
                    new UniformLoadSetRecord(){ Name = ELoadCaseType.ConstructionLiveLCa.ToString(), Load = cll },
                    new UniformLoadSetRecord(){ Name = ELoadCaseType.MassDeadLCa.ToString(), Load = massDl },
                    //Live loads are special:
                    new UniformLoadSetRecord(){ Name = llType.ToString(), Load = liveLoad}
                }
            };

            return loadSet;

            /***************************************************/
        }
    }
}

