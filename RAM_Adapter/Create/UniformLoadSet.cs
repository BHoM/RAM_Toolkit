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
using BH.oM.Spatial.SettingOut;
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
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<UniformLoadSet> loadSets)
        {
            foreach (UniformLoadSet loadSet in loadSets)
            {
                try
                {
                    ISurfaceLoadPropertySets ramSurfaceLoadPropertySets = m_Model.GetSurfaceLoadPropertySets();

                    int existingLoadPropSetID = 0;

                    //Check if load set already exists
                    for (int i = 0; i < ramSurfaceLoadPropertySets.GetCount(); i++)
                    {
                        ISurfaceLoadPropertySet ramPropSet = ramSurfaceLoadPropertySets.GetAt(i);
                        if (ramPropSet.strLabel == loadSet.Name)
                        {
                            existingLoadPropSetID = ramPropSet.lUID;
                        }
                    }

                    if (existingLoadPropSetID == 0)
                    {
                        //Add the loadset if it does not already exist
                        ISurfaceLoadPropertySet ramLoadSet = ramSurfaceLoadPropertySets.Add(loadSet.Name);

                        int liveCount = 0;

                        foreach (UniformLoadSetRecord loadRecord in loadSet.Loads)
                        {
                            switch (loadRecord.Name)
                            {
                                case "ConstructionDeadLCa":
                                    ramLoadSet.dConstDeadLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    break;
                                case "ConstructionLiveLCa":
                                    ramLoadSet.dConstLiveLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    break;
                                case "DeadLCa":
                                    ramLoadSet.dDeadLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    break;
                                case "MassDeadLCa":
                                    ramLoadSet.dMassDeadLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    break;
                                case "PartitionLCa":
                                    ramLoadSet.dPartitionLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    break;
                                case "LiveLCa":
                                    ramLoadSet.eLiveLoadType = ELoadCaseType.LiveReducibleLCa;
                                    ramLoadSet.dLiveLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    liveCount++;
                                    break;
                                case "LiveStorageLCa":
                                    ramLoadSet.eLiveLoadType = ELoadCaseType.LiveStorageLCa;
                                    ramLoadSet.dLiveLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    liveCount++;
                                    break;
                                case "LiveUnReducibleLCa":
                                    ramLoadSet.eLiveLoadType = ELoadCaseType.LiveUnReducibleLCa;
                                    ramLoadSet.dLiveLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    liveCount++;
                                    break;
                                case "LiveRoofLCa":
                                    ramLoadSet.eLiveLoadType = ELoadCaseType.LiveRoofLCa;
                                    ramLoadSet.dLiveLoad = loadRecord.Load.ToKilopoundForcePerSquareInch();
                                    liveCount++;
                                    break;
                                default:
                                    Engine.Base.Compute.RecordWarning($"the record {loadRecord.Name} in {loadSet.Name} was not recognized. Create your UniformLoadSet using CreateRAMUniformLoadSet() in the RAM toolkit!");
                                    break;
                            }
                        };

                        if (liveCount > 1)
                        {
                            Engine.Base.Compute.RecordWarning("More than one live load has been set; only the last one will be applied");
                        }

                        //Set the custom data to return if created
                        RAMId RAMId = new RAMId();
                        RAMId.Id = ramLoadSet.lUID;
                        loadSet.SetAdapterId(RAMId);
                    }
                    else
                    {
                        //Set the custom data to return if already existing
                        RAMId RAMId = new RAMId();
                        RAMId.Id = existingLoadPropSetID;
                        loadSet.SetAdapterId(RAMId);
                    }
                }
                catch
                {
                    CreateElementError("UniformLoadSet", loadSet.Name);
                }
            }

            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }

        /***************************************************/
    }
}

