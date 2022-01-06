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
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Results;
using BH.oM.Structure.Loads;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.Requests;
using BH.oM.Analytical.Results;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.Engine.Base;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private List<Loadcase> ReadLoadCase(List<string> ids = null)
        {
            //Implement code for reading loadcases
            List<Loadcase> bhomLoadCases = new List<Loadcase>();

            ILoadCases ILoadCases = m_Model.GetLoadCases(EAnalysisResultType.RAMFrameResultType);

            for (int i = 0; i < ILoadCases.GetCount(); i++)
            {
                //Get Loadcases
                ILoadCase LoadCase = ILoadCases.GetAt(i);
                Loadcase bhomLoadcase = BH.Adapter.RAM.Convert.ToBHoMObject(LoadCase);
                bhomLoadCases.Add(bhomLoadcase);
            }

            return bhomLoadCases;

        }

        /***************************************************/

        private List<LoadCombination> ReadLoadCombination(List<string> ids = null)
        {
            //Implement code for reading loadcombinations
            List<LoadCombination> bhomLoadCombinations = new List<LoadCombination>();

            ILoadCombinations ILoadCombinations = m_Model.GetLoadCombinations(COMBO_MATERIAL_TYPE.ANALYSIS_CUSTOM);

            for (int i = 0; i < ILoadCombinations.GetCount(); i++)
            {
                //Get LoadCombinations
                ILoadCombination ILoadCombination = ILoadCombinations.GetAt(i);
                LoadCombination bhomLoadCombination = BH.Adapter.RAM.Convert.ToBHoMObject(m_Model, ILoadCombination);
                bhomLoadCombinations.Add(bhomLoadCombination);
            }

            return bhomLoadCombinations;

        }

        /***************************************************/

        private List<RAMPointGravityLoad> ReadPointGravityLoad(List<string> ids = null)
        {

            //Implement code for reading Gravity Loads
            List<RAMPointGravityLoad> bhomPtGravLoads = new List<RAMPointGravityLoad>();

            IGravityLoads1 ramGravityLoads = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IGravityLoads_INT);

            // Get all IWalls
            List<IWall> allRamWalls = ReadRamWalls(m_Model);

            // Adding node reactions per wall per gravity loads (point and line)
            foreach (IWall wall in allRamWalls)
            {
                int ramWallID = wall.lUID;
                int numLineLoads = 0;
                int numPointLoads = 0;

                double pdDist, pdDL, pdCDL, pdCLL, pdLLPosRed, pdLLNegRed, pdLLPosNonRed, pdLLNegNonRed, pdLLPosStorage, pdLLNegStorage,
                pdLLPosRoof, pdLLNegRoof, pdPosPL, pdNegPL, pdAxDL, pdAxCDL, pdAxCLL, pdAxNedRefLL, pdAxPosRedLL, pdAxNegNonRedLL, pdAxPosNonRedLL,
                pdAxNegStorageLL, pdAxPosStorageLL, pdAxNegRoofLL, pdAxPosRoofLL, pdAxNegPL, pdAxPosPL, pdPosLLRF, pdNegLLRF,
                pdPosStorageLLRF, pdNegStorageLLRF, pdPosRoofLLRF, pdNegRoofLLRF;

                pdDist = pdDL = pdCDL = pdCLL = pdLLPosRed = pdLLNegRed = pdLLPosNonRed = pdLLNegNonRed = pdLLPosStorage = pdLLNegStorage =
                pdLLPosRoof = pdLLNegRoof = pdPosPL = pdNegPL = pdAxDL = pdAxCDL = pdAxCLL = pdAxNedRefLL = pdAxPosRedLL = pdAxNegNonRedLL = pdAxPosNonRedLL =
                pdAxNegStorageLL = pdAxPosStorageLL = pdAxNegRoofLL = pdAxPosRoofLL = pdAxNegPL = pdAxPosPL = pdPosLLRF = pdNegLLRF =
                pdPosStorageLLRF = pdNegStorageLLRF = pdPosRoofLLRF = pdNegRoofLLRF = 0;

                EGRAVPTLOADSOURCE peLoadType = EGRAVPTLOADSOURCE.EPtLoadFromGravBmReact;

                ramGravityLoads.GetNumWallLoads(ramWallID, ref numLineLoads, ref numPointLoads);

                for (int i = 0; i < numPointLoads; i++)
                {
                    ramGravityLoads.GetWallPointLoad2(ramWallID, i, ref pdDist, ref pdDL, ref pdCDL, ref pdCLL, ref pdLLPosRed, ref pdLLNegRed, ref pdLLPosNonRed, ref pdLLNegNonRed, ref pdLLPosStorage, ref pdLLNegStorage,
                    ref pdLLPosRoof, ref pdLLNegRoof, ref pdPosPL, ref pdNegPL, ref pdAxDL, ref pdAxCDL, ref pdAxCLL, ref pdAxNedRefLL, ref pdAxPosRedLL, ref pdAxNegNonRedLL, ref pdAxPosNonRedLL,
                    ref pdAxNegStorageLL, ref pdAxPosStorageLL, ref pdAxNegRoofLL, ref pdAxPosRoofLL, ref pdAxNegPL, ref pdAxPosPL, ref pdPosLLRF, ref pdNegLLRF,
                    ref pdPosStorageLLRF, ref pdNegStorageLLRF, ref pdPosRoofLLRF, ref pdNegRoofLLRF, ref peLoadType);
                    RAMPointGravityLoad bhomPtGravLoad = new RAMPointGravityLoad
                    {
                        ObjectId = wall.lUID,
                        dist = pdDist,
                        DL = pdDL,
                        NonRLL = pdLLPosNonRed,
                        RedLL = pdLLPosRed,
                        RoofLL = pdLLPosRoof,
                        StorLL = pdLLPosStorage,
                        type = peLoadType.ToString()
                    };
                    bhomPtGravLoads.Add(bhomPtGravLoad);
                }
            }

            // Get all IBeams
            List<IBeam> allRamBeams = ReadRamBeams(m_Model);

            // Adding node reactions per beam per gravity loads (point and line)
            foreach (IBeam beam in allRamBeams)
            {
                int ramBeamID = beam.lUID;
                int numLineLoads = 0;
                int numPointLoads = 0;

                double pdDist, pdDL, pdCDL, pdCLL, pdLLPosRed, pdLLNegRed, pdLLPosNonRed, pdLLNegNonRed, pdLLPosStorage, pdLLNegStorage,
                pdLLPosRoof, pdLLNegRoof, pdPosPL, pdNegPL, pdAxDL, pdAxCDL, pdAxCLL, pdAxNegRedLL, pdAxPosRedLL, pdAxNegNonRedLL, pdAxPosNonRedLL,
                pdAxNegStorageLL, pdAxPosStorageLL, pdAxNegRoofLL, pdAxPosRoofLL, pdAxNegPL, pdAxPosPL, pdPosLLRF, pdNegLLRF,
                pdPosStorageLLRF, pdNegStorageLLRF, pdPosRoofLLRF, pdNegRoofLLRF;

                pdDist = pdDL = pdCDL = pdCLL = pdLLPosRed = pdLLNegRed = pdLLPosNonRed = pdLLNegNonRed = pdLLPosStorage = pdLLNegStorage =
                pdLLPosRoof = pdLLNegRoof = pdPosPL = pdNegPL = pdAxDL = pdAxCDL = pdAxCLL = pdAxNegRedLL = pdAxPosRedLL = pdAxNegNonRedLL = pdAxPosNonRedLL =
                pdAxNegStorageLL = pdAxPosStorageLL = pdAxNegRoofLL = pdAxPosRoofLL = pdAxNegPL = pdAxPosPL = pdPosLLRF = pdNegLLRF =
                pdPosStorageLLRF = pdNegStorageLLRF = pdPosRoofLLRF = pdNegRoofLLRF = 0;

                EGRAVPTLOADSOURCE peLoadSource = EGRAVPTLOADSOURCE.EPtLoadFromGravBmReact;

                ramGravityLoads.GetNumBeamLoads(ramBeamID, ref numLineLoads, ref numPointLoads);

                for (int i = 0; i < numPointLoads; i++)
                {
                    ramGravityLoads.GetBeamPointLoad(ramBeamID, i, ref pdDist, ref pdDL, ref pdCDL, ref pdCLL, ref pdLLPosRed, ref pdLLNegRed, ref pdLLPosNonRed,
                        ref pdLLNegNonRed, ref pdLLPosStorage, ref pdLLNegStorage, ref pdLLPosRoof, ref pdLLNegRoof, ref pdAxDL, ref pdAxCDL, ref pdAxCLL,
                        ref pdAxNegRedLL, ref pdAxPosRedLL, ref pdAxNegNonRedLL, ref pdAxPosNonRedLL, ref pdAxNegStorageLL, ref pdAxPosStorageLL, ref pdAxNegRoofLL,
                        ref pdAxPosRoofLL, ref pdPosLLRF, ref pdNegLLRF, ref pdPosStorageLLRF, ref pdNegStorageLLRF, ref pdPosRoofLLRF, ref pdNegRoofLLRF, ref peLoadSource);
                    RAMPointGravityLoad bhomPtGravLoad = new RAMPointGravityLoad
                    {
                        ObjectId = ramBeamID,
                        dist = pdDist,
                        DL = pdDL,
                        NonRLL = pdLLPosNonRed,
                        RedLL = pdLLPosRed,
                        RoofLL = pdLLPosRoof,
                        StorLL = pdLLPosStorage,
                        type = peLoadSource.ToString()
                    };
                    bhomPtGravLoads.Add(bhomPtGravLoad);
                }
            }

            return bhomPtGravLoads;
        }

        /***************************************************/

        private List<RAMLineGravityLoad> ReadLineGravityLoad(List<string> ids = null)
        {

            //Implement code for reading Gravity Loads
            List<RAMLineGravityLoad> bhomLineGravLoads = new List<RAMLineGravityLoad>();

            IModel ramModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            IGravityLoads1 ramGravityLoads = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IGravityLoads_INT);

            // Get all IWalls
            List<IWall> allRamWalls = ReadRamWalls(ramModel);

            // Adding node reactions per wall per gravity loads (point and line)
            foreach (IWall wall in allRamWalls)
            {
                int ramWallID = wall.lUID;
                int numLineLoads = 0;
                int numPointLoads = 0;

                double pdDistL, pdDistR, pdDLL, pdDLR, pdCDLL, pdCDLR, pdLLL, pdLLR, pdPLL, pdPLR, pdCLLL, pdCLLR,
                    pdAxDLL, pdAxDLR, AxCDLL, pdAxCDLR, pdAxCLLL, pdAxCLLR, pdAxLLL, pdAxLLR, pdAxPLL, pdAxPLR, pdRfactor;

                pdDistL = pdDistR = pdDLL = pdDLR = pdCDLL = pdCDLR = pdLLL = pdLLR = pdPLL = pdPLR = pdCLLL = pdCLLR =
                    pdAxDLL = pdAxDLR = AxCDLL = pdAxCDLR = pdAxCLLL = pdAxCLLR = pdAxLLL = pdAxLLR = pdAxPLL = pdAxPLR = pdRfactor = 0;

                EGRAVLOADTYPE peLoadType = EGRAVLOADTYPE.ENonRedLoad;

                ramGravityLoads.GetNumWallLoads(ramWallID, ref numLineLoads, ref numPointLoads);

                for (int i = 0; i < numLineLoads; i++)
                {
                    ramGravityLoads.GetWallLineLoad2(ramWallID, i, ref pdDistL, ref pdDistR, ref pdDLL, ref pdDLR, ref pdCDLL, ref pdCDLR, ref pdLLL, ref pdLLR, ref pdPLL, ref pdPLR, ref pdCLLL, ref pdCLLR,
                    ref pdAxDLL, ref pdAxDLR, ref AxCDLL, ref pdAxCDLR, ref pdAxCLLL, ref pdAxCLLR, ref pdAxLLL, ref pdAxLLR, ref pdAxPLL, ref pdAxPLR, ref peLoadType, ref pdRfactor);
                    RAMLineGravityLoad bhomLineGravLoad = new RAMLineGravityLoad
                    {
                        ObjectId = wall.lUID,
                        dist1 = pdDistL,
                        dist2 = pdDistR,
                        DL1 = pdDLL,
                        DL2 = pdDLR,
                        LL1 = pdLLL,
                        LL2 = pdLLR,
                        PL1 = pdPLL,
                        PL2 = pdPLR,
                        type = peLoadType.ToString()
                    };
                    bhomLineGravLoads.Add(bhomLineGravLoad);
                }
            }

            // Get all IBeams
            List<IBeam> allRamBeams = ReadRamBeams(ramModel);

            // Adding node reactions per Beam per gravity loads (point and line)
            foreach (IBeam beam in allRamBeams)
            {
                int ramBeamID = beam.lUID;
                int numLineLoads = 0;
                int numPointLoads = 0;

                double pdDistL, pdDistR, pdDLL, pdDLR, pdCDLL, pdCDLR, pdLLL, pdLLR, pdPLL, pdPLR, pdCLLL, pdCLLR,
                    pdAxDLL, pdAxDLR, pdAxCDLL, pdAxCDLR, pdAxCLLL, pdAxCLLR, pdAxLLL, pdAxLLR, pdAxPLL, pdAxPLR, pdRfactor;

                pdDistL = pdDistR = pdDLL = pdDLR = pdCDLL = pdCDLR = pdLLL = pdLLR = pdPLL = pdPLR = pdCLLL = pdCLLR =
                    pdAxDLL = pdAxDLR = pdAxCDLL = pdAxCDLR = pdAxCLLL = pdAxCLLR = pdAxLLL = pdAxLLR = pdAxPLL = pdAxPLR = pdRfactor = 0;

                EGRAVLOADTYPE peLoadType = EGRAVLOADTYPE.ENonRedLoad;

                ramGravityLoads.GetNumBeamLoads(ramBeamID, ref numLineLoads, ref numPointLoads);

                for (int i = 0; i < numLineLoads; i++)
                {
                    ramGravityLoads.GetBeamLineLoad(ramBeamID, i, ref pdDistL, ref pdDistR, ref pdDLL, ref pdDLR, ref pdCDLL, ref pdCDLR,
                        ref pdLLL, ref pdLLR, ref pdCLLL, ref pdCLLR, ref pdAxDLL, ref pdAxDLR, ref pdAxCDLL, ref pdAxCDLR, ref pdAxCLLL,
                        ref pdAxCLLR, ref pdAxLLL, ref pdAxLLR, ref peLoadType, ref pdRfactor);
                    RAMLineGravityLoad bhomLineGravLoad = new RAMLineGravityLoad
                    {
                        ObjectId = ramBeamID,
                        dist1 = pdDistL,
                        dist2 = pdDistR,
                        DL1 = pdDLL,
                        DL2 = pdDLR,
                        LL1 = pdLLL,
                        LL2 = pdLLR,
                        PL1 = pdPLL,
                        PL2 = pdPLR,
                        type = peLoadType.ToString()
                    };
                    bhomLineGravLoads.Add(bhomLineGravLoad);
                }
            }

            return bhomLineGravLoads;
        }
        
        /***************************************************/

    }

}



