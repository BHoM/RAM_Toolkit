﻿using System;
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
        public static UniformLoadSet CreateRAMUniformLoadSet(double sdl, double cdl, double liveLoad, int llType, double partition, double cll, double massDl, string name = "")
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
                case 0:
                    loadSet.Loads[ELoadCaseType.LiveReducibleLCa.ToString()] = liveLoad;
                    break;
                case 1:
                    loadSet.Loads[ELoadCaseType.LiveStorageLCa.ToString()] = liveLoad;
                    break;
                case 2:
                    loadSet.Loads[ELoadCaseType.LiveUnReducibleLCa.ToString()] = liveLoad;
                    break;
                case 3:
                    loadSet.Loads[ELoadCaseType.LiveRoofLCa.ToString()] = liveLoad;
                    break;
                default:
                    Engine.Reflection.Compute.RecordWarning("Could not understand llType. 0 = Reducible, 1 = Storage, 2 = Non-reducible, 3 = Roof");
                    break;
            }

            return loadSet;

            /***************************************************/
        }
    }
}