/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.Engine.RAM;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Results;
using BH.oM.Structure.Loads;
using BH.oM.Common.Materials;
using BH.oM.Structure.MaterialFragments;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Architecture.Elements;



namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            //Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            if (type == typeof(Bar))
                return ReadBars(ids as dynamic);
            else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
                return ReadSectionProperties(ids as dynamic);
            else if (type == typeof(IMaterialFragment))
                return ReadMaterials(ids as dynamic);
            else if (type == typeof(Panel))
                return ReadPanels(ids as dynamic);
            else if (type == typeof(ISurfaceProperty))
                return ReadISurfaceProperties(ids as dynamic);
            else if (type == typeof(LoadCombination))
                return ReadLoadCombination(ids as dynamic);
            else if (type == typeof(Loadcase))
                return ReadLoadCase(ids as dynamic);
            else if (type == typeof(Level))
                return ReadLevel(ids as dynamic);
            else if (type == typeof(NodeReaction))
                return ReadNodeReaction(ids as dynamic);
            if (type == typeof(Grid))
                return ReadGrid(ids as dynamic);


            return null;
        }

        /***************************************************/
        /**** Private specific read methods             ****/
        /***************************************************/

        //The List<string> in the methods below can be changed to a list of any type of identification more suitable for the toolkit

        private List<Bar> ReadBars(List<string> ids = null)
        {
            //Implement code for reading bars
            List<Bar> bhomBars = new List<Bar>();
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            // Get stories
            IStories IStories = IModel.GetStories();
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
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IColumn);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam IBeam = IBeams.GetAt(j);
                    ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IBeam, ILayoutBeam, dElevation);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Vertical Braces
                for (int j = 0; j < numVBraces; j++)
                {
                    IVerticalBrace IVerticalBrace = IVBraces.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IVerticalBrace);
                    bhomBar.CustomData["FloorType"] = IFloorType.strLabel;
                    bhomBars.Add(bhomBar);
                }

                // Convert Horizontal Braces
                for (int j = 0; j < numHBraces; j++)
                {
                    IHorizBrace IHorizBrace = IHorizBraces.GetAt(j);
                    ILayoutHorizBrace ILayoutHorizBrace = ILayoutHorizBraces.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IHorizBrace, ILayoutHorizBrace, dElevation);
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

            Material defaultbhomMat = new Material();

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
            //Implement code for reading materials

            List<ISurfaceProperty> IProps = new List<ISurfaceProperty>();

            //Material steel = BMaterialType.Steel, 210000, 0.3, 0.00012, 78500);

            ISurfaceProperty IProp = (ISurfaceProperty) new ConstantThickness();
            IProp.Name = "default";
            //IProp.Type = MaterialType.Concrete;

            IProps.Add(IProp);

            return IProps;

            //throw new NotImplementedException();

        }

        /***************************************************/

        private List<Loadcase> ReadLoadCase(List<string> ids = null)
        {
            //Implement code for reading loadcases
            List<Loadcase> bhomLoadCases = new List<Loadcase>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            ILoadCases ILoadCases = IModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);

            for (int i = 0; i < ILoadCases.GetCount(); i++)
            {
                //Get Loadcases
                ILoadCase LoadCase = ILoadCases.GetAt(i);
                Loadcase bhomLoadcase = BH.Engine.RAM.Convert.ToBHoMObject(LoadCase);
                bhomLoadCases.Add(bhomLoadcase);
            }

            return bhomLoadCases;

        }

        /***************************************************/

        private List<LoadCombination> ReadLoadCombination(List<string> ids = null)
        {
            //Implement code for reading loadcombinations
            List<LoadCombination> bhomLoadCombinations = new List<LoadCombination>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            ILoadCombinations ILoadCombinations = IModel.GetLoadCombinations(COMBO_MATERIAL_TYPE.ANALYSIS_CUSTOM);

            for (int i = 0; i < ILoadCombinations.GetCount(); i++)
            {
                //Get LoadCombinations
                ILoadCombination ILoadCombination = ILoadCombinations.GetAt(i);
                LoadCombination bhomLoadCombination = BH.Engine.RAM.Convert.ToBHoMObject(IModel, ILoadCombination);
                bhomLoadCombinations.Add(bhomLoadCombination);
            }

            return bhomLoadCombinations;

        }

        /***************************************************/

        private List<Panel> ReadPanels(List<string> ids = null)
        {
            //Implement code for reading panels
            List<Panel> bhomPanels = new List<Panel>();
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Get stories
            IStories IStories = IModel.GetStories();
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
                    Panel Panel = BH.Engine.RAM.Convert.ToBHoMObject(IWall);
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
                        Panel Panel = BH.Engine.RAM.Convert.ToBHoMObject(IDeck, IModel, IStoryUID);
                        bhomPanels.Add(Panel);
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

        private List<Grid> ReadGrid(List<string> ids = null)
        {
            //Implement code for reading Grids
            List<Grid> bhomGrids = new List<Grid>();
 
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
                 
            // Get the gridsystems that are in the model
            IGridSystems IGridSystems = IModel.GetGridSystems();
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

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            // Get the levels that are in the model

            IStories stories = IModel.GetStories();
            int numStories = stories.GetCount();

            for (int i = 0; i < numStories; i++)
            {
                bhomLevels.Add(stories.GetAt(i).ToBHoMObject());
            }

            return bhomLevels;
        }

        /***************************************************/

        private List<NodeReaction> ReadNodeReaction(List<string> ids = null)
        {
            //Implement code for reading Node Reactions
            List<NodeReaction> bhomNodeReactions = new List<NodeReaction>();

            IModel ramModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            ILoadCases ramLoadCases = ramModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);

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

            // Adding node reactions for Walls per wall per loadcase
            foreach (IWall wall in allRamWalls)
            {

                for (int i = 0; i < ramLoadCases.GetCount(); i++)
                {
                    //Get Loadcases
                    ILoadCase ramLoadCase = ramLoadCases.GetAt(i);
                    IPointLoads wallNodeForces = wall.GetNodeForcesAtEdge(EAnalysisResultType.RAMFrameResultType, ramLoadCase.lUID, EEdge.eBottomEdge);

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

    }

}
