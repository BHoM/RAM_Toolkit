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
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry.SettingOut;
using BH.Engine.Units;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> IRead(Type type, IList ids, ActionConfig actionConfig = null)
        {
            dynamic elems = null;
            //Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            if (type == typeof(Bar))
                elems = ReadBars(ids as dynamic);
            else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
                elems = ReadSectionProperties(ids as dynamic);
            else if (type == typeof(IMaterialFragment))
                elems = ReadMaterials(ids as dynamic);
            else if (type == typeof(Panel))
                elems = ReadPanels(ids as dynamic);
            else if (type == typeof(ISurfaceProperty))
                elems = ReadISurfaceProperties(ids as dynamic);
            else if (type == typeof(LoadCombination))
                elems = ReadLoadCombination(ids as dynamic);
            else if (type == typeof(Loadcase))
                elems = ReadLoadCase(ids as dynamic);
            else if (type == typeof(Level))
                elems = ReadLevel(ids as dynamic);
            else if (type == typeof(Grid))
                elems = ReadGrid(ids as dynamic);
            else if (type == typeof(RAMPointGravityLoad))
                elems = ReadPointGravityLoad(ids as dynamic);
            else if (type == typeof(RAMLineGravityLoad))
                elems = ReadLineGravityLoad(ids as dynamic);
            else if (type == typeof(RAMFactoredEndReactions))
                elems = ReadBeamEndReactions(ids as dynamic);
            else if (type == typeof(ContourLoadSet))
                elems = ReadContourLoadSets(ids as dynamic);
            else if (type == typeof(UniformLoadSet))
                elems = ReadUniformLoadSets(ids as dynamic);

            return elems;
        }

        /***************************************************/



        /***************************************************/

        /***************************************************/
        /**** Private specific read methods             ****/
        /***************************************************/

        //The List<string> in the methods below can be changed to a list of any type of identification more suitable for the toolkit

        private List<Bar> ReadBars(List<string> ids = null)
        {
            //Implement code for reading bars
            List<Bar> bhomBars = new List<Bar>();

            // Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Columns
                IColumns IColumns = IStories.GetAt(i).GetColumns();
                int numColumns = IColumns.GetCount();

                //Get Beams
                IFloorType IFloorType = IStories.GetAt(i).GetFloorType();
                ILayoutBeams ILayoutBeams = IFloorType.GetLayoutBeams();
                IBeams IBeams = IStories.GetAt(i).GetBeams();
                int numLayoutBeams = ILayoutBeams.GetCount();
                int numBeams = IBeams.GetCount();

                //Get Vertical Braces
                IVerticalBraces IVBraces = IStories.GetAt(i).GetVerticalBraces();
                int numVBraces = IVBraces.GetCount();

                //Get Horizontal Braces
                ILayoutHorizBraces ILayoutHorizBraces = IStories.GetAt(i).GetFloorType().GetLayoutHorizBraces();
                IHorizBraces IHorizBraces = IStories.GetAt(i).GetHorizBraces();
                int numHBraces = ILayoutHorizBraces.GetCount();

                //Get Elevation
                double dElevation = IStories.GetAt(i).dElevation;

                // Convert Columns
                for (int j = 0; j < numColumns; j++)
                {
                    IColumn IColumn = IColumns.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IColumn);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam IBeam = IBeams.GetAt(j);
                    ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IBeam, ILayoutBeam, dElevation);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Vertical Braces
                for (int j = 0; j < numVBraces; j++)
                {
                    IVerticalBrace IVerticalBrace = IVBraces.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IVerticalBrace);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Horizontal Braces
                for (int j = 0; j < numHBraces; j++)
                {
                    IHorizBrace IHorizBrace = IHorizBraces.GetAt(j);
                    ILayoutHorizBrace ILayoutHorizBrace = ILayoutHorizBraces.GetAt(j);
                    Bar bhomBar = BH.Adapter.RAM.Convert.ToBHoMObject(IHorizBrace, ILayoutHorizBrace, dElevation);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }
            }

            return bhomBars;

        }

        /***************************************/

        private List<ISectionProperty> ReadSectionProperties(List<string> ids = null)
        {

            List<ISectionProperty> ISectionProperties = new List<ISectionProperty>();

            //Material defaultbhomMat = new Material();

            ISectionProperty sec2b = new ExplicitSection();
            //sec2b.Material = BH.Engine.Common.Create.Material("otherSteel", MaterialType.Steel, 210000, 0.3, 0.00012, 78500);
            sec2b.Name = "Section 2b";

            ISectionProperties.Add(sec2b);

            return ISectionProperties;

            //throw new NotImplementedException();
        }

        /***************************************/

        private List<IMaterialFragment> ReadMaterials(List<string> ids = null)
        {
            //TODO: Implement code for reading materials from RAM if not handled per element

            List<IMaterialFragment> Materials = new List<IMaterialFragment>();

            IMaterialFragment defaultMat = Engine.Structure.Create.Steel("Default");

            Materials.Add(defaultMat);

            return Materials;
        }

        /***************************************************/

        private List<ISurfaceProperty> ReadISurfaceProperties(List<string> ids = null)
        {

            List<ISurfaceProperty> propList = new List<ISurfaceProperty>();
            ISteelCriteria steelCriteria = m_Model.GetSteelCriteria();
            IDeckTableEntries deckProfiles = steelCriteria.GetDeckTableEntries();

            ICompDeckProps compDeckProps = m_Model.GetCompositeDeckProps();
            for (int i = 0; i < compDeckProps.GetCount(); i++)
            {
                ICompDeckProp DeckProp = compDeckProps.GetAt(i);
                string deckLabel = DeckProp.strLabel;
                string deckProfileName = DeckProp.strDeckType;
                IDeckTableEntry profile = null;

                for (int j = 0; j < deckProfiles.GetCount(); j++) // find ram deck profile to get props
                {
                    profile = deckProfiles.GetAt(j);
                    if (profile.strDeckName == deckLabel)
                    { break;}
                }

                double concThickness = DeckProp.dThickAboveFlutes.FromInch();
                double deckProfileThickness = profile.dTD.FromInch();
                double deckThickness = concThickness + deckProfileThickness;

                IMaterialFragment material = Engine.Structure.Create.Concrete("Concrete Over Deck");

                Ribbed deck2DProp = new Ribbed();
                deck2DProp.Name = deckLabel;
                deck2DProp.Thickness = concThickness;
                deck2DProp.PanelType = PanelType.Slab;
                deck2DProp.Material = material;
                deck2DProp.CustomData[AdapterIdName] = DeckProp.lUID;
                deck2DProp.CustomData["DeckProfileName"] = deckProfileName;
                deck2DProp.Spacing = profile.dRSpac;
                deck2DProp.StemWidth = profile.dWR;
                deck2DProp.TotalDepth = deckThickness;
                propList.Add(deck2DProp);
            }

            IConcSlabProps concSlabProps = m_Model.GetConcreteSlabProps();
            for (int i = 0; i < concSlabProps.GetCount(); i++)
            {
                IConcSlabProp DeckProp = concSlabProps.GetAt(i);
                double deckThickness = DeckProp.dThickness.FromInch();
                string deckLabel = DeckProp.strLabel;
                IMaterialFragment material = Engine.Structure.Create.Concrete("Concrete");

                ConstantThickness deck2DProp = new ConstantThickness();
                deck2DProp.CustomData[AdapterIdName] = DeckProp.lUID;
                deck2DProp.Name = deckLabel;
                deck2DProp.Material = material;
                deck2DProp.Thickness = deckThickness;
                deck2DProp.PanelType = PanelType.Slab;
                propList.Add(deck2DProp);
            }

            INonCompDeckProps nonCompDeckProps = m_Model.GetNonCompDeckProps();
            for (int i = 0; i < nonCompDeckProps.GetCount(); i++)
            {
                INonCompDeckProp DeckProp = nonCompDeckProps.GetAt(i);
                double deckThickness = DeckProp.dEffectiveThickness.FromInch();
                string deckLabel = DeckProp.strLabel;
                IMaterialFragment material = Engine.Structure.Create.Steel("Metal Deck");

                ConstantThickness deck2DProp = new ConstantThickness();
                deck2DProp.CustomData[AdapterIdName] = DeckProp.lUID;
                deck2DProp.Name = deckLabel;
                deck2DProp.Material = material;
                deck2DProp.Thickness = deckThickness;
                deck2DProp.PanelType = PanelType.Slab;
                propList.Add(deck2DProp);
            }

            return propList;
        }

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

        private List<Panel> ReadPanels(List<string> ids = null)
        {
            //Get dictionary of surface properties with ids
            Dictionary<string, ISurfaceProperty> bhomProperties = ReadISurfaceProperties().ToDictionary(x => x.CustomData[AdapterIdName].ToString());

            //Implement code for reading panels
            List<Panel> bhomPanels = new List<Panel>();

            //Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {

                //Get Walls
                IWalls IWalls = IStories.GetAt(i).GetWalls();
                int numWalls = IWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall IWall = IWalls.GetAt(j);
                    Panel Panel = BH.Adapter.RAM.Convert.ToBHoMObject(IWall);
                    bhomPanels.Add(Panel);
                }

                //Get Floors
                IStory IStory = IStories.GetAt(i);
                IFloorType IFloorType = IStory.GetFloorType();
                IDecks IDecks = IFloorType.GetDecks();
                int IStoryUID = IStory.lUID;

                int numDecks = IDecks.GetCount();

                // Convert Floors
                for (int j = 0; j < numDecks; j++)
                {
                    IDeck IDeck = IDecks.GetAt(j);
                    try
                    {
                        Panel panel = BH.Adapter.RAM.Convert.ToBHoMObject(IDeck, m_Model, IStoryUID);
                        panel.Property = bhomProperties[IDeck.lPropID.ToString()];
                        bhomPanels.Add(panel);
                    }
                    catch
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("This story has no slab edges defined. IStoryUID: " + IStoryUID);
                    }
                }
            }

            return bhomPanels;
        }

        /***************************************************/

        private List<ContourLoadSet> ReadContourLoadSets(List<string> ids = null)
        {
            //Implement code for reading Contour Load Sets
            List<ContourLoadSet> bhomContourLoadSets = new List<ContourLoadSet>();
            Dictionary<int, UniformLoadSet> bhomUniformLoadSets = ReadUniformLoadSets().ToDictionary(x => (int)x.CustomData[AdapterIdName]);

            //Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {
                IFloorType floorType = IStories.GetAt(i).GetFloorType();
                //Get contour load sets per story
                ISurfaceLoadSets srfLoadSets = floorType.GetSurfaceLoadSets2();
                int numSrfLoads = srfLoadSets.GetCount();

                for (int j = 0; j<numSrfLoads; j++)
                {
                    ISurfaceLoadSet srfLoadSet = srfLoadSets.GetAt(j);
                    ContourLoadSet srfLoad = srfLoadSet.ToBHoMObject(IStories.GetAt(i));
                    int propUID = srfLoadSet.lPropertySetUID;
                    srfLoad.UniformLoadSet = bhomUniformLoadSets[propUID];
                    bhomContourLoadSets.Add(srfLoad);
                }
            }

            return bhomContourLoadSets;
        }

        /***************************************************/

        private List<UniformLoadSet> ReadUniformLoadSets(List<string> ids = null)
        {
            //Implement code for reading Contour Load Sets
            List<UniformLoadSet> bhomUniformLoadSets = new List<UniformLoadSet>();

            ISurfaceLoadPropertySets RAMLoadSets = m_Model.GetSurfaceLoadPropertySets();
            
            for (int i = 0; i < RAMLoadSets.GetCount(); i++)
            {
                UniformLoadSet bhLoad = RAMLoadSets.GetAt(i).ToBHoMObject();
                bhomUniformLoadSets.Add(bhLoad);
            }

            return bhomUniformLoadSets;
        }

        /***************************************************/

        private List<Grid> ReadGrid(List<string> ids = null)
        {
            //Implement code for reading Grids
            List<Grid> bhomGrids = new List<Grid>();
                 
            // Get the gridsystems that are in the model
            IGridSystems IGridSystems = m_Model.GetGridSystems();
            int numGridSystems = IGridSystems.GetCount();
     
            // Get all elements on each GridSystem
            for (int i = 0; i < numGridSystems; i++)
            {
                //Look into a specific gridsystem
                IGridSystem myGridSystem = IGridSystems.GetAt(i);

                //get the amoount of gridlines that are in the system
                IModelGrids IModelGrids = myGridSystem.GetGrids();
                
                int numGridLines = IModelGrids.GetCount();

                // Loop through all gridlines in the GridSystem and add a bhomGrid
                 for (int j = 0; j < numGridLines; j++)
                {
                    IModelGrid IModelGrid = IModelGrids.GetAt(j);
                    Grid bhomGrid = IModelGrid.ToBHoMObject(myGridSystem, j);
                    bhomGrids.Add(bhomGrid);
                }
            }

            return bhomGrids;
        }

        /***************************************************/

        private List<Level> ReadLevel(List<string> ids = null)
        {
            //Implement code for reading Levels
            List<Level> bhomLevels = new List<Level>();

            // Get the levels that are in the model

            IStories stories = m_Model.GetStories();
            int numStories = stories.GetCount();

            for (int i = 0; i < numStories; i++)
            {
                bhomLevels.Add(stories.GetAt(i).ToBHoMObject());
            }

            return bhomLevels;
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

        private List<NodeReaction> ReadNodeReaction(List<string> ids = null)
        {

            //Implement code for reading Node Reactions
            List<NodeReaction> bhomNodeReactions = new List<NodeReaction>();

            IModel ramModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ILoadCases ramLoadCases = ramModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);
            //Get IWalls
            List<IWall> allRamWalls = ReadRamWalls(ramModel);

            // Adding node reactions per wall per loadcase, this is node reactions at btm of wall
            foreach (IWall wall in allRamWalls)
            {

                for (int i = 0; i < ramLoadCases.GetCount(); i++)
                {
                    //Get Loadcases
                    ILoadCase ramLoadCase = ramLoadCases.GetAt(i);
                    IPointLoads wallNodeForces = wall.GetNodeForcesAtEdge(EAnalysisResultType.RAMGravityResultType, ramLoadCase.lUID, EEdge.eBottomEdge);
                    for (int j = 0; j < wallNodeForces.GetCount(); j++)
                    {
                        //Get Node Forces
                        IPointLoad wallNodeForce = wallNodeForces.GetAt(j);
                        NodeReaction bhomNodeReaction = wallNodeForce.ToBHoMObject(ramLoadCase);
                        bhomNodeReactions.Add(bhomNodeReaction);
                    }
                }
            }

            return bhomNodeReactions;
        }

        /***************************************************/

        private List<BarDeformation> ReadBarDeformations(List<string> ids = null)
        {
            List<BarDeformation> bhomBarDeformations = new List<BarDeformation> ();
            List <IBeam> ramBeams= ReadRamBeams(m_Model);
            foreach (IBeam ramBeam in ramBeams)
            {
                int beamID = ramBeam.lUID;
                //TODO: read deflections to go here            
            }
            return bhomBarDeformations;
        }


        /***************************************************/

        private List<RAMFactoredEndReactions> ReadBeamEndReactions(List<string> ids = null)
        {
            List<RAMFactoredEndReactions> barEndReactions = new List<RAMFactoredEndReactions>();

            IModel ramModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            List<IBeam> ramBeams = ReadRamBeams(ramModel);

            foreach ( IBeam beam in ramBeams)
            {
                IAnalyticalResult result = beam.GetAnalyticalResult();
                IMemberForces forces = result.GetMaximumComboReactions(COMBO_MATERIAL_TYPE.GRAV_STEEL);

                RAMFactoredEndReactions bhomEndReactions = new RAMFactoredEndReactions()
                {
                    ObjectId = beam.lUID,
                    StartReaction = forces.GetAt(0).ToBHoMObject(),
                    EndReaction = forces.GetAt(1).ToBHoMObject(),
                };

                barEndReactions.Add(bhomEndReactions);
            }

            return barEndReactions;
        }

        /***************************************************/

        private List<IWall> ReadRamWalls(IModel ramModel)
        {
            //Get stories
            IStories ramStories = ramModel.GetStories();
            int numStories = ramStories.GetCount();
            List<IWall> allRamWalls = new List<IWall>();

            // Get all walls on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Walls
                IWalls ramWalls = ramStories.GetAt(i).GetWalls();
                int numWalls = ramWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall ramWall = ramWalls.GetAt(j);
                    allRamWalls.Add(ramWall);
                }
            }

            return allRamWalls;
        }

        /***************************************************/

        private List<IBeam> ReadRamBeams(IModel ramModel)
        {
            //Get stories
            IStories ramStories = ramModel.GetStories();
            int numStories = ramStories.GetCount();
            List<IBeam> allRamBeams = new List<IBeam>();

            // Get all beams on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get beams
                IBeams ramBeams = ramStories.GetAt(i).GetBeams();
                int numBeams = ramBeams.GetCount();

                // Convert beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam ramBeam = ramBeams.GetAt(j);
                    allRamBeams.Add(ramBeam);
                }
            }

            return allRamBeams;
        }

        /***************************************************/

    }

}

