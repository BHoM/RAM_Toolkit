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


namespace BH.Engine.Adapters.RAM
{
    public static partial class Create
    {
        public static UniformLoadSet CreateRAMUniformLoadSet(double sdl, double cdl, double liveLoad, RAMLiveLoadTypes llType, double partition, double cll, double massDl, string name = "")
        {

            UniformLoadSet loadSet = new UniformLoadSet
            {
                Name = name,
                Loads = new Dictionary<string, double>
                {
                    { ELoadCaseType.DeadLCa.ToString(), sdl },
                    { ELoadCaseType.ConstructionDeadLCa.ToString(), cdl },
                    { ELoadCaseType.PartitionLCa.ToString(), partition },
                    { ELoadCaseType.ConstructionLiveLCa.ToString(), cll },
                    { ELoadCaseType.MassDeadLCa.ToString(), massDl }
                }
            };

            switch (llType)
            {
                case RAMLiveLoadTypes.LiveReducibleLCa:
                    loadSet.Loads[ELoadCaseType.LiveReducibleLCa.ToString()] = liveLoad;
                    break;
                case RAMLiveLoadTypes.LiveStorageLCa:
                    loadSet.Loads[ELoadCaseType.LiveStorageLCa.ToString()] = liveLoad;
                    break;
                case RAMLiveLoadTypes.LiveUnReducibleLCa:
                    loadSet.Loads[ELoadCaseType.LiveUnReducibleLCa.ToString()] = liveLoad;
                    break;
                case RAMLiveLoadTypes.LiveRoofLCa:
                    loadSet.Loads[ELoadCaseType.LiveRoofLCa.ToString()] = liveLoad;
                    break;
                default:
                    Engine.Reflection.Compute.RecordWarning("Could not understand llType. 0 = Reducible, 1 = Storage, 2 = Non-reducible, 3 = Roof. Assumed Live Reducible.");
                    loadSet.Loads[ELoadCaseType.LiveReducibleLCa.ToString()] = liveLoad;
                    break;
            }

            return loadSet;

            /***************************************************/
        }
    }
}

