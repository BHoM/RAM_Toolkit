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
using BH.oM.Structure.Elements;
using BH.oM.Structure.Properties.Section;
using BH.oM.Structure.Properties.Surface;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.Engine.Structure;
using BH.oM.Architecture.Elements;
using BH.Engine.RAM;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        private object get;

        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = true)
        {
            bool success = true;        //boolean returning if the creation was successfull or not

            // Create objects per type
            success = CreateCollection(objects as dynamic);

            return success;             //Finally return if the creation was successful or not

        }


        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<Bar> bhomBars)
        {
            //Code for creating a collection of bars in the software
            List<Bar> bars = bhomBars.ToList();

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Get the stories in the model
            IStories ramStories = IModel.GetStories();

            //Cycle through bars, add bar to appropriate story.
            foreach (Bar bar in bars)
            {
                IStory startStory = bar.StartNode.GetStory(ramStories);
                IStory endStory = bar.EndNode.GetStory(ramStories);

                if (startStory == endStory)
                {
                    double xStart = bar.StartNode.Position().X;
                    double yStart = bar.StartNode.Position().Y;
                    double zStart = bar.StartNode.Position().Z - startStory.dElevation;
                    double xEnd = bar.EndNode.Position().X;
                    double yEnd = bar.EndNode.Position().Y;
                    double zEnd = bar.EndNode.Position().Z - endStory.dElevation;
                    
                    IFloorType ramFloorType = startStory.GetFloorType();
                    ILayoutBeams ramBeams = ramFloorType.GetLayoutBeams();
                    ILayoutBeam ramBeam = ramBeams.Add(bar.SectionProperty.Material.ToRAM(), xStart, yStart, zStart, xEnd, yEnd, zEnd); //toDelete was 5

                    IBeams beamsOnStory = startStory.GetBeams();
                    IBeam beam = beamsOnStory.Get(ramBeam.lUID);
                    beam.strSectionLabel = bar.SectionProperty.Name;
                    beam.EAnalyzeFlag = EAnalyzeFlag.eAnalyze;
                }
                else
                {
                    double xStart = bar.StartNode.Position().X;
                    double yStart = bar.StartNode.Position().Y;
                    double zStart = bar.StartNode.Position().Z - startStory.dElevation;
                    double xEnd = bar.EndNode.Position().X;
                    double yEnd = bar.EndNode.Position().Y;
                    double zEnd = bar.EndNode.Position().Z - endStory.dElevation;

                    IFloorType ramFloorType = endStory.GetFloorType();
                    ILayoutColumns ramColumns = ramFloorType.GetLayoutColumns();
                    ILayoutColumn ramColumn;

                    if (bar.IsVertical())
                    {
                        //Failing if no section property is provided
                        ramColumn = ramColumns.Add(bar.SectionProperty.Material.ToRAM(), xEnd, yEnd, zEnd, zStart);
                    }
                    else
                    {
                        ramColumn = ramColumns.Add2(bar.SectionProperty.Material.ToRAM(), xEnd, yEnd, xStart, yStart, zEnd, zStart);
                    }

                    //Set column properties
                    IColumns colsOnStory = endStory.GetColumns();
                    IColumn column = colsOnStory.Get(ramColumn.lUID);
                    column.strSectionLabel = bar.SectionProperty.Name;
                    column.EAnalyzeFlag = EAnalyzeFlag.eAnalyze;
                }
            }

            //Save file
            RAMDataAccIDBIO.SaveDatabase();

            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            //System.IO.File.Delete(filePathUserfile);
            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<ISectionProperty> sectionProperties)
        {
            //Code for creating a collection of section properties in the software

            //Not yet implemented

            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Material> materials)
        {
            //Code for creating a collection of materials in the software

            //Not yet implemented

            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<ISurfaceProperty> ISurfaceProperties)
        {           
            //NOTE: Deck property functionality not resolved yet but code framework is below

            ////Access model
            //IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ////Get composite deck properties
            //ICompDeckProps ICompDeckProps = IModel.GetCompositeDeckProps();

            //foreach (ISurfaceProperty iProp in ISurfaceProperties)
            //{
            //    string deckName = iProp.Name;
            //    double thickness = 6;
            //    double studLength = 4;

            //    ICompDeckProps.Add(deckName, thickness, studLength);

            //    object iPropId = iProp.CustomData[AdapterId];
            //}

            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<PanelPlanar> bhomPanels)
        {
            //Code for creating a collection of floors and walls in the software

            List<PanelPlanar> panels = bhomPanels.ToList();

            // Register Floor types
            IFloorType IFloorType;
            IStories IStories;
            IStory IStory;

            //Create wall and floor lists with individual heights
            List<PanelPlanar> wallPanels = new List<PanelPlanar>();
            List<PanelPlanar> floors = new List<PanelPlanar>();
            List<double> panelHeights = new List<double>();
            List<Point> panelPoints = new List<Point>();

            // Split walls and floors and get all elevations
            foreach (PanelPlanar panel in panels)
            {
                List<double> thisPanelHeights = new List<double>();
                
                // Get heights of wall and floor corners to create levels
                PolyCurve panelOutline = Engine.Structure.Query.Outline(panel);
                panelPoints = panelOutline.DiscontinuityPoints();

                foreach (Point pt in panelPoints)
                {
                    panelHeights.Add(Math.Round(pt.Z, 0));
                    thisPanelHeights.Add(Math.Round(pt.Z, 0));
                }

                double panelHeight = thisPanelHeights.Max() - thisPanelHeights.Min();
                
                //Split walls and floors
                if (panelHeight>0.1)
                {
                    wallPanels.Add(panel);
                }
                else
                {
                    floors.Add(panel);
                }
            }

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            IStories = IModel.GetStories();

            //Get concrete deck properties
            IConcSlabProps IConcSlabProps = IModel.GetConcreteSlabProps();

            // Cycle through floortypes, access appropriate story, place panels on those stories
            for (int i = 0; i < IStories.GetCount(); i++)
            {
                IStory = IStories.GetAt(i);
                IFloorType = IStory.GetFloorType();
                //Cycle through floors; if z of panel = the floor height, add it
                for (int j = 0; j < floors.Count(); j++)
                {
                    PanelPlanar floor = floors[j];

                    // Get coords of corner points of floor outline to check if floor elevation = panel elevation
                    List<Point> ctrlPointsCheck = new List<Point>();
                    ctrlPointsCheck = BH.Engine.Structure.Query.ControlPoints(floor, true);

                    // If on level, add deck to IDecks for that level
                    if (Math.Round(ctrlPointsCheck[0].Z,0) == IStory.dElevation)
                    {
                        //Create list of external and internal panel outlines
                        List<PolyCurve> panelOutlines = new List<PolyCurve>();

                        // Get external and internal adges of floor panel
                        PolyCurve outlineExternal = floor.Outline();
                        panelOutlines.Add(outlineExternal);
                        List<Opening> panelOpenings = floor.Openings;

                        foreach (Opening opening in panelOpenings)
                        {
                            PolyCurve outlineOpening = opening.Outline();
                            panelOutlines.Add(outlineOpening);
                        }

                        // Set slab edges on FloorType in RAM for external edges
                        ISlabEdges ISlabEdges = IFloorType.GetAllSlabEdges();
                        Vector zDown = BH.Engine.Geometry.Create.Vector(0, 0, -1);

                        foreach (PolyCurve outline in panelOutlines)
                        {
                            // RAM requires edges clockwise, flip if counterclockwise
                            PolyCurve cwOutline = (outline.IsClockwise(zDown) == false) ? outline.Flip() : outline;

                            List<ICurve> edgeCrvs = cwOutline.Curves;

                            foreach (ICurve crv in edgeCrvs)
                            {
                                Point startPt = crv.IStartPoint();
                                Point endPt = crv.IEndPoint();
                                ISlabEdges.Add(startPt.X, startPt.Y, endPt.X, endPt.Y, 0);
                            }
                        }


                        //// Create Deck (IDecks.Add causes RAMDataAccIDBIO to be read only causing crash, slab edges only for now)

                        //IDecks IDecks = IFloorType.GetDecks();
                        //IDeck IDeck = null;

                        //// Default panel properties to apply to model
                        //string deckName = "Default RAM_Toolkit"; //pull deck name from decktable
                        //double thickness = 8;
                        //double selfweight = 150;
                        //IConcSlabProp = IConcSlabProps.Add(deckName, thickness, selfweight);
                        //IDeck = IDecks.Add(IConcSlabProp.lUID, ctrlPoints.Count); // THIS CAUSES READ MEMORY ERROR CRASHING AT SAVE
                        //IPoints IPoints = IDeck.GetPoints();

                        //// Create list of SCoordinates for floor outlines
                        //List<SCoordinate> cornersExt = new List<SCoordinate>();

                        //foreach (Point point in ctrlPointsExternal)
                        //{
                        //    SCoordinate cornerExt = BH.Engine.RAM.Convert.ToRAM(point);
                        //    cornersExt.Add(corner);
                        //}

                        //for (int k = 0; k < cornersExt.Count; k++)
                        //{
                        //    IPoints.Delete(k);
                        //    IPoints.InsertAt(k, cornersExt[k]);
                        //}

                    }
                }

                //Cycle through walls; if top of wall is at floor height add wall to FloorType
                for (int j = 0; j < wallPanels.Count(); j++)
                {

                    PanelPlanar wallPanel = wallPanels[j];

                    // Default Thickness for now
                    double thickness = 6;

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Structure.Query.Outline(wallPanel);
                    BoundingBox wallBounds = BH.Engine.Geometry.Query.Bounds(outline);
                    Point wallMin = wallBounds.Min;
                    Point wallMax = wallBounds.Max;

                    // If wall crosses level, add wall to ILayoutWalls for that level
                    if (Math.Round(wallMax.Z, 0) >= IStory.dElevation && Math.Round(wallMin.Z, 0) <= IStory.dElevation)
                    {
                        //Get ILayoutWalls of FloorType
                        ILayoutWalls ILayoutWalls = IFloorType.GetLayoutWalls();

                        ILayoutWalls.Add(EMATERIALTYPES.EWallPropConcreteMat, wallMin.X, wallMin.Y, 0, 0, wallMax.X, wallMax.Y, 0, 0, thickness);
                    }
                }
            }

            //Save file
            RAMDataAccIDBIO.SaveDatabase();

            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            //System.IO.File.Delete(filePathUserfile);

            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Level> bhomLevels)
        {
            //sort levels by elevation
            IOrderedEnumerable<Level> sortedBhomLevels = bhomLevels.OrderBy(o => o.Elevation);
            
            //Check levels for negatives
            if (sortedBhomLevels.First().Elevation < 0)
            {
                throw new Exception("Base level can not be negative for RAM. Please move model origin point to set all geometry and levels at 0 or greater.");
            }

            // Register Floor types
            IFloorTypes ramFloorTypes;
            IFloorType ramFloorType;
            IStories ramStories;
                        
            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Create floor type at each level

            for (int i = 0; i < sortedBhomLevels.Count(); i++)
            {
                Level level = sortedBhomLevels.ElementAt(i);

                double height;
                // Ground floor ht = 0 for RAM
                if (i == 0) { height = level.Elevation; }
                else
                {
                    Level lastLevel = sortedBhomLevels.ElementAt(i - 1);
                    height = level.Elevation - lastLevel.Elevation;
                }
                
                ramStories = IModel.GetStories();
                List<double> ramElevs = new List<double>();
                for (int j = 0; j < ramStories.GetCount(); j++)
                {
                    ramElevs.Add(ramStories.GetAt(j).dElevation);
                }

                int newIndex = Math.Max(ramElevs.Count(), ramElevs.FindIndex(x => x > level.Elevation));

                ramFloorTypes = IModel.GetFloorTypes();
                ramFloorType = ramFloorTypes.Add(level.Name);

                // Insert story at index
                ramStories.InsertAt(newIndex,ramFloorType.lUID, level.Name, height);
                
            }

            //Save file
            RAMDataAccIDBIO.SaveDatabase();
            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Grid> bhomGrid)
        {
            //Code for creating a Grid System in the software

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);


            // Register GridSystems
            IGridSystems IGridSystems = IModel.GetGridSystems();

            // Register FloorTypes
            IFloorTypes myFloorTypes = IModel.GetFloorTypes();

            // initializa a BhoM grid
            List<Grid> Grids = bhomGrid.ToList();

            //Split grids by gridtypes
            List<Grid> XGrids = new List<Grid>();
            List<Grid> YGrids = new List<Grid>();
            List<Grid> skewGrids = new List<Grid>();
            List<Grid> circGrids = new List<Grid>();
            Grid grid = new Grid();
            Polyline gridLine = new Polyline();

            //create different names for the gridSystem based on if there are items in the list
            double gridSystemRotation = 0;
            string gridSystemLabel = "";
            IGridSystem IGridSystemXY = null;
            IModelGrids IModelGridsXY = null;
            IModelGrids IModelGridsRad = null;



            //Loop through the BHoM grids and sort per type (x,y,radial, circular, skewed) 
            for (int i = 0; i < Grids.Count(); i++)
            {
                grid = Grids[i];

                if (grid.Curve is Circle)
                {
                    circGrids.Add(grid);
                }
                else
                {
                    gridLine = Engine.Geometry.Modify.CollapseToPolyline(grid.Curve as dynamic, 10);
                    //add lines to corresponding lists (XGrids, YGrids) based on their  orientation
                    if (Math.Abs(gridLine.StartPoint().X - gridLine.EndPoint().X) < 0.1)
                    {
                        YGrids.Add(grid);
                    }
                    else if (Math.Abs(gridLine.StartPoint().Y - gridLine.EndPoint().Y) < 0.1)
                    {
                        XGrids.Add(grid);
                    }
                    else
                    {
                        skewGrids.Add(grid);
                    }
                }
            }


            //Create grid systems per grid lists

            //XYGrids
            if (YGrids.Count() != 0 || XGrids.Count() != 0)
            {
                 gridSystemLabel = "XY_grid";
                 IGridSystemXY = IGridSystems.Add(gridSystemLabel);
                 IGridSystemXY.eOrientationType = SGridSysType.eGridOrthogonal;
                 IGridSystemXY.dRotation = gridSystemRotation;
                 IModelGridsXY = IGridSystemXY.GetGrids();
            }


            // NOTE: Radial and Skewed Not Yet Implemented but code framework is below

            ////Radial Circular Grid
            //if (circGrids.Count() != 0)
            //{
            //    gridSystemLabel = "Radial_grid";
            //    IGridSystemRad = IGridSystems.Add(gridSystemLabel);
            //    IGridSystemRad.dXOffset = gridOffsetX;
            //    IGridSystemRad.dYOffset = gridOffsetY;
            //    IGridSystemRad.eOrientationType = SGridSysType.eGridRadial;
            //    IGridSystemRad.dRotation = gridSystemRotation;
            //    IModelGridsRad = IGridSystemRad.GetGrids();
            //}
            //// Skewed grid
            //if (skewGrids.Count() != 0) {
            //    gridSystemLabel = "Skew_gird";
            //    IGridSystemSk = IGridSystems.Add(gridSystemLabel);
            //    IGridSystemSk.dXOffset = 0;
            //    IGridSystemSk.dYOffset = 0;
            //    IGridSystemSk.eOrientationType = SGridSysType.eGridSkewed;
            //    IGridSystemSk.dRotation = gridSystemRotation;
            //    IModelGridsSk = IGridSystemSk.GetGrids();

            //}


            //  Get Grid System Offset
            double minY = XGrids[0].Curve.IStartPoint().Y;
            double minX = YGrids[0].Curve.IStartPoint().X;

            foreach (Grid XGrid in XGrids)
            {
                double gridY = XGrid.Curve.IStartPoint().Y;
                if (gridY < minY)
                    minY = gridY;
            }

            foreach (Grid YGrid in YGrids)
            {
                double gridX = YGrid.Curve.IStartPoint().X;
                if (gridX < minX)
                    minX = gridX;
            }
            IGridSystemXY.dXOffset = minX;
            IGridSystemXY.dYOffset = minY;


            // Create Grids in GridSystem
            foreach (Grid XGrid in XGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(XGrid.Curve as dynamic, 10);
                IModelGridsXY.Add(XGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y-minY);
            }

            foreach (Grid YGrid in YGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(YGrid.Curve as dynamic, 10);
                IModelGridsXY.Add(YGrid.Name, EGridAxis.eGridXorRadialAxis, gridLine.StartPoint().X-minX);
            }

            foreach (Grid cGrid in circGrids)
            {

                IModelGridsRad.Add(cGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y);
                // TODO: add code to implement circular grids
                // Create GridSystem in RAM for each unique centerpt of circGrids  

            }

            foreach (Grid skGrid in skewGrids)
            {
                // TODO: add code to implement skewed grids
                // Create GridSystem in RAM for each unique angle of skewGrids

            }

            //get the ID of the gridsystem
            int gridSystemID = IGridSystemXY.lUID;


            //TODO: Assign grid system to all floor types
            //Create a default floor type and assign the newly created gridsystem
            //string defFloorTypeName = "Default_floorType";
            //IFloorType myFloorType = myFloorTypes.Add(defFloorTypeName);
            //IStories myStories = IModel.GetStories();


            ////Cycle through floortypes, access the existing floortype/story, place grids on those stories
            //for (int i = 0; i < myFloorTypes.GetCount(); i++)
            //    {
            //        myFloorType = myFloorTypes.GetAt(i);
            //        IStory myStory= myStories.GetAt(i);
            //        DAArray gsID = myFloorType.GetGridSystemIDArray();
            //        gsID.Add(IGridSystemXY.lUID, 0);
            //        myFloorType.SetGridSystemIDArray(gsID);
            //    }



            //Save file
            RAMDataAccIDBIO.SaveDatabase();
            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            return true;
        }

        /***************************************************/

    }
}
