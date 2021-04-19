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


namespace BH.Engine.Adapters.RAM
{
    public static partial class Create
    {
        [PreviousVersion("4.2","BH.Engine.Adapters.RAM.Create.CreateRAMUniformLoadSet(System.Double, System.Double, System.Double, BH.oM.Adapters.RAM.RAMLiveLoadTypes, System.Double, System.Double, System.Double, System.String)")]
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

