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
using BH.oM.Architecture.Elements;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Constraints;
using BH.oM.Structure.MaterialFragments;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.Engine.Structure;
using BH.Engine.RAM;



namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
      
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = true)
        {
            bool success = true;        //boolean returning if the creation was successful or not

            // Create objects per type
            if (objects.Count() > 0)
            {

                success = CreateCollection(objects as dynamic);

            }
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

            //Cycle through bars, split to beam and col lists, then add to corresponding story.
            List<Bar> barBeams = new List<Bar>();
            List<Bar> barCols = new List<Bar>();

            foreach (Bar testBar in bars)
            {
                bool isBeam = Math.Abs(testBar.Tangent(true).DotProduct(Vector.ZAxis)) < 0.5;

                if (isBeam) { barBeams.Add(testBar); }
                else { barCols.Add(testBar); }
            }

            //Create beams per story, flat
            foreach (Bar bar in barBeams)
            {
                string name = bar.Name;

                try
                {
                    IStory barStory = bar.GetStory(StructuralUsage1D.Beam, ramStories);

                    double xStart = bar.StartNode.Position().X;
                    double yStart = bar.StartNode.Position().Y;
                    double zStart = bar.StartNode.Position().Z - barStory.dElevation;
                    double xEnd = bar.EndNode.Position().X;
                    double yEnd = bar.EndNode.Position().Y;
                    double zEnd = bar.EndNode.Position().Z - barStory.dElevation;

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutBeams ramBeams = ramFloorType.GetLayoutBeams();
                    ILayoutBeam ramBeam = ramBeams.Add(bar.SectionProperty.Material.ToRAM(), xStart, yStart, 0, xEnd, yEnd, 0); // No Z offsets, beams flat on closest story

                    // Add warning to report distance of snapping to level as required for RAM
                    if (zStart != 0 || zEnd != 0)
                    {Engine.Reflection.Compute.RecordWarning("Bar " + name + " snapped to level " + barStory.strLabel + ". Bar moved " + Math.Round(zStart,2).ToString() + " at start and " + Math.Round(zEnd,2).ToString() + " at end."); }

                    IBeams beamsOnStory = barStory.GetBeams();
                    IBeam beam = beamsOnStory.Get(ramBeam.lUID);
                    beam.strSectionLabel = bar.SectionProperty.Name;
                    // beam.EAnalyzeFlag = EAnalyzeFlag.eAnalyze; deprecated in API
                }
                catch (Exception ex)
                {
                    CreateElementError("bar", name);
                }
            }

            //Create columns at each story with offset per actual height
            foreach (Bar bar in barCols)
            {
                string name = bar.Name;

                try
                {
                    IStory barStory = bar.GetStory(StructuralUsage1D.Column, ramStories);
                    
                    List<Node> colNodes = new List<Node>() { bar.StartNode, bar.EndNode };
                    colNodes.Sort((x, y) => x.Position.Z.CompareTo(y.Position.Z));

                    double xBtm = colNodes[0].Position.X;
                    double yBtm = colNodes[0].Position.Y;
                    double zBtm = colNodes[0].Position.Z - barStory.dElevation;
                    double xTop = colNodes[1].Position.X;
                    double yTop = colNodes[1].Position.Y;
                    double zTop = colNodes[1].Position.Z - barStory.dElevation + barStory.dFlrHeight;

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutColumns ramColumns = ramFloorType.GetLayoutColumns();
                    ILayoutColumn ramColumn;

                    if (bar.IsVertical())
                    {
                        //Failing if no section property is provided
                        ramColumn = ramColumns.Add(bar.SectionProperty.Material.ToRAM(), xTop, yTop, 0, 0); //No Z offsets, cols start and end at stories
                    }
                    else
                    {
                        ramColumn = ramColumns.Add2(bar.SectionProperty.Material.ToRAM(), xTop, yTop, xBtm, yBtm, 0, 0); //No Z offsets, cols start and end at stories
                    }

                    //Set column properties
                    IColumns colsOnStory = barStory.GetColumns();
                    IColumn column = colsOnStory.Get(ramColumn.lUID);
                    column.strSectionLabel = bar.SectionProperty.Name;
                    column.EAnalyzeFlag = EAnalyzeFlag.eAnalyze;
                }
                catch (Exception ex)
                {
                    CreateElementError("bar", name);
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

        private bool CreateCollection(IEnumerable<IMaterialFragment> materials)
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

        private bool CreateCollection(IEnumerable<Panel> bhomPanels)
        {
            //Code for creating a collection of floors and walls in the software

            List<Panel> panels = bhomPanels.ToList();

            // Register Floor types
            IFloorType ramFloorType;
            IStories ramStories;
            IStory ramStory;

            //Create wall and floor lists with individual heights
            List<Panel> wallPanels = new List<Panel>();
            List<Panel> floors = new List<Panel>();
            List<double> panelHeights = new List<double>();
            List<Point> panelPoints = new List<Point>();

            // Split walls and floors and get all elevations
            foreach (Panel panel in panels)
            {
                double panelNormZ = panel.Normal().Z;

                //Split walls and floors
                if (Math.Abs(panelNormZ)<0.707) // check normal against 45 degree slope
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
            IModel ramModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ramStories = ramModel.GetStories();

            //Get concrete deck properties
            IConcSlabProps ramConcSlabProps = ramModel.GetConcreteSlabProps();

            // Cycle through floors and create on story
            foreach (Panel panel in floors)
            {
                string name = panel.Name;

                try
                {
                    ramStory = panel.GetStory(ramStories);
                    ramFloorType = ramStory.GetFloorType();

                    // Set slab edges on FloorType in RAM for external edges
                    ISlabEdges ramSlabEdges = ramFloorType.GetAllSlabEdges();
                    ISlabEdges ramOpeningEdges = ramFloorType.GetAllSlabOpenings();

                    // Get external and internal edges of floor panel
                    List<PolyCurve> panelOutlines = new List<PolyCurve>();
                    List<PolyCurve> openingOutlines = new List<PolyCurve>();

                    PolyCurve outlineExternal = panel.Outline();
                    List<Opening> panelOpenings = panel.Openings;

                    foreach (Opening opening in panelOpenings)
                    {
                        PolyCurve outlineOpening = opening.Outline();
                        openingOutlines.Add(outlineOpening);
                    }

                    Vector zDown = BH.Engine.Geometry.Create.Vector(0, 0, -1);

                    // RAM requires edges clockwise, flip if counterclockwise
                    PolyCurve cwOutline = (outlineExternal.IsClockwise(zDown) == false) ? outlineExternal.Flip() : outlineExternal;

                    List<ICurve> edgeCrvs = cwOutline.Curves;

                    foreach (ICurve crv in edgeCrvs)
                    {
                        Point startPt = crv.IStartPoint();
                        Point endPt = crv.IEndPoint();
                        ramSlabEdges.Add(startPt.X, startPt.Y, endPt.X, endPt.Y, 0);
                    }

                    foreach (PolyCurve outline in openingOutlines)
                    {
                        // RAM requires edges clockwise, flip if counterclockwise
                        PolyCurve cwOpenOutline = (outline.IsClockwise(zDown) == false) ? outline.Flip() : outline;

                        List<ICurve> openEdgeCrvs = cwOpenOutline.Curves;

                        foreach (ICurve crv in openEdgeCrvs)
                        {
                            Point startPt = crv.IStartPoint();
                            Point endPt = crv.IEndPoint();
                            ramOpeningEdges.Add(startPt.X, startPt.Y, endPt.X, endPt.Y, 0);
                        }
                    }

                    // Add warning to report floors flattened to level as required for RAM
                    if (Math.Abs(panel.Normal().Z) < 1)
                    { Engine.Reflection.Compute.RecordWarning("Panel " + name + " snapped to level " + ramStory.strLabel + "."); }
                }
                catch (Exception ex)
                {
                    CreateElementError("panel", name);
                }


                //// Create Deck (IDecks.Add causes RAMDataAccIDBIO to be read only causing crash, slab edges only for now)

                //IDecks ramDecks = ramFloorType.GetDecks();
                //IDeck ramDeck = null;

                //// Default panel properties to apply to model
                //string deckName = "Default RAM_Toolkit Deck"; //pull deck name from decktable
                //double deckThk = 6.5;
                //double selfweight = 150;
                //IConcSlabProp ramConcSlabProp = ramConcSlabProps.Add(deckName, deckThk, selfweight);
                //List<Point> ctrlPoints = outlineExternal.ControlPoints();
                //ramDeck = ramDecks.Add(ramConcSlabProp.lUID, ctrlPoints.Count); // THIS CAUSES READ MEMORY ERROR CRASHING AT SAVE
                //IPoints ramPoints = ramDeck.GetPoints();

                //// Create list of SCoordinates for floor outlines
                //List<SCoordinate> cornersExt = new List<SCoordinate>();

                //foreach (Point point in ctrlPoints)
                //{
                //    SCoordinate cornerExt = BH.Engine.RAM.Convert.ToRAM(point);
                //    cornersExt.Add(cornerExt);
                //}

                //for (int k = 0; k < cornersExt.Count; k++)
                //{
                //    ramPoints.Delete(k);
                //    ramPoints.InsertAt(k, cornersExt[k]);
                //}
            }
            

            //Cycle through walls; if wall crosses level place at level
            foreach (Panel wallPanel in wallPanels)
                {
                string name = wallPanel.Name;

                try
                {
                    // Default Thickness for now
                    double thickness = 6;

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Structure.Query.Outline(wallPanel);
                    BoundingBox wallBounds = BH.Engine.Geometry.Query.Bounds(outline);
                    Point wallMin = wallBounds.Min;
                    Point wallMax = wallBounds.Max;

                    for (int i = 0; i < ramStories.GetCount(); i++)
                    {
                        ramStory = ramStories.GetAt(i);
                        // If wall crosses level, add wall to ILayoutWalls for that level
                        if (Math.Round(wallMax.Z, 0) >= ramStory.dElevation && Math.Round(wallMin.Z, 0) < ramStory.dElevation)
                        {
                            ramFloorType = ramStory.GetFloorType();
                            //Get ILayoutWalls of FloorType
                            ILayoutWalls ramLayoutWalls = ramFloorType.GetLayoutWalls();

                            ramLayoutWalls.Add(EMATERIALTYPES.EWallPropConcreteMat, wallMin.X, wallMin.Y, 0, 0, wallMax.X, wallMax.Y, 0, 0, thickness);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CreateElementError("panel", name);
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
            if (bhomLevels.Count() != 0)
            {
                //sort levels by elevation
                IOrderedEnumerable<Level> orderedBhomLevels = bhomLevels.OrderBy(o => o.Elevation);
                List<Level> sortedBhomLevels = new List<Level>();

                //Check levels for negatives
                if (orderedBhomLevels.First().Elevation < 0)
                {
                    throw new Exception("Base level can not be negative for RAM. Please move model origin point to set all geometry and levels at 0 or greater.");
                }

                //Check levels for base level = 0, remove if occurs
                if (orderedBhomLevels.First().Elevation == 0)
                {
                    sortedBhomLevels = orderedBhomLevels.Where(level => level.Elevation != 0).ToList();
                }
                else
                {
                    sortedBhomLevels = orderedBhomLevels.Where(level => level.Elevation != 0).ToList();
                }

                // Register Floor types
                IFloorTypes ramFloorTypes;
                IFloorType ramFloorType = null;
                IStories ramStories;

                //Access model
                IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

                //Create floor type at each level
                for (int i = 0; i < sortedBhomLevels.Count(); i++)
                {
                    Level level = sortedBhomLevels.ElementAt(i);

                    // Get elevations and skip if level elevation already in RAM
                    ramStories = IModel.GetStories();
                    List<double> ramElevs = new List<double>();
                    for (int j = 0; j < ramStories.GetCount(); j++)
                    {
                        ramElevs.Add(ramStories.GetAt(j).dElevation);
                    }

                    if (ramElevs.Contains(level.Elevation) != true)
                    {
                        double height;
                        // Ground floor ht = 0 for RAM
                        if (i == 0)
                        {
                            height = level.Elevation;
                        }
                        else
                        {
                            Level lastLevel = sortedBhomLevels.ElementAt(i - 1);
                            height = level.Elevation - lastLevel.Elevation;
                        }

                        int newIndex;
                        if (ramElevs.FindIndex(x => x > level.Elevation) == -1)
                        {
                            newIndex = ramElevs.Count();
                        }
                        else
                        {
                            newIndex = ramElevs.FindIndex(x => x > level.Elevation);
                        }

                        List<string> ramFloorTypeNames = new List<string>();
                        ramFloorTypes = IModel.GetFloorTypes();
                        Boolean floorTypeExists = false;
                        for (int j = 0; j < ramFloorTypes.GetCount(); j++)
                        {
                            if (ramFloorTypes.GetAt(j).strLabel == level.Name)
                            {
                                ramFloorType = ramFloorTypes.GetAt(j);
                                floorTypeExists = true;
                            }
                        }

                        if (floorTypeExists == false)
                        {
                            ramFloorType = ramFloorTypes.Add(level.Name);
                        }

                        // Modify story above if not top floor
                        if (newIndex < ramStories.GetCount())
                        {
                            IStory ramStoryAbove = ramStories.GetAt(newIndex);
                            ramStoryAbove.dFlrHeight = ramStoryAbove.dElevation - level.Elevation;
                        }
                        if (newIndex > 0 && ramStories.GetCount() > 0)
                        {
                            IStory ramStoryBelow = ramStories.GetAt(newIndex - 1);
                            height = level.Elevation - ramStoryBelow.dElevation;

                        }

                        // Insert story at index
                        ramStories.InsertAt(newIndex, ramFloorType.lUID, level.Name, height);
                    }

                }

                //Save file
                RAMDataAccIDBIO.SaveDatabase();
                // Release main interface and delete user file
                RAMDataAccIDBIO = null;

            }

            return true;

        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Grid> bhomGrid)
        {
            //Code for creating a Grid System in the software

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel ramModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);


            // Register GridSystems
            IGridSystems ramGridSystems = ramModel.GetGridSystems();

            // Register FloorTypes
            IFloorTypes ramFloorTypes = ramModel.GetFloorTypes();

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
            IGridSystem ramGridSystemXY = null;
            IGridSystem ramGridSystemRad = null;
            IGridSystem ramGridSystemSk = null;
            IModelGrids ramModelGridsXY = null;
            IModelGrids ramModelGridsRad = null;
            IModelGrids ramModelGridsSk = null;



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
                 ramGridSystemXY = ramGridSystems.Add(gridSystemLabel);
                 ramGridSystemXY.eOrientationType = SGridSysType.eGridOrthogonal;
                 ramGridSystemXY.dRotation = gridSystemRotation;
                 ramModelGridsXY = ramGridSystemXY.GetGrids();
            }


            // NOTE: Radial and Skewed Not Yet Implemented but code framework is below

            ////Radial Circular Grid
            //if (circGrids.Count() != 0)
            //{
            //    gridSystemLabel = "Radial_grid";
            //    ramGridSystemRad = ramGridSystems.Add(gridSystemLabel);
            //    ramGridSystemRad.dXOffset = gridOffsetX;
            //    ramGridSystemRad.dYOffset = gridOffsetY;
            //    ramGridSystemRad.eOrientationType = SGridSysType.eGridRadial;
            //    ramGridSystemRad.dRotation = gridSystemRotation;
            //    ramModelGridsRad = ramGridSystemRad.GetGrids();
            //}
            //// Skewed grid
            //if (skewGrids.Count() != 0) {
            //    gridSystemLabel = "Skew_gird";
            //    ramGridSystemSk = ramGridSystems.Add(gridSystemLabel);
            //    ramGridSystemSk.dXOffset = 0;
            //    ramGridSystemSk.dYOffset = 0;
            //    ramGridSystemSk.eOrientationType = SGridSysType.eGridSkewed;
            //    ramGridSystemSk.dRotation = gridSystemRotation;
            //    ramModelGridsSk = ramGridSystemSk.GetGrids();

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
            ramGridSystemXY.dXOffset = minX;
            ramGridSystemXY.dYOffset = minY;


            // Create Grids in GridSystem
            foreach (Grid XGrid in XGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(XGrid.Curve as dynamic, 10);
                ramModelGridsXY.Add(XGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y-minY);
            }

            foreach (Grid YGrid in YGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(YGrid.Curve as dynamic, 10);
                ramModelGridsXY.Add(YGrid.Name, EGridAxis.eGridXorRadialAxis, gridLine.StartPoint().X-minX);
            }

            foreach (Grid cGrid in circGrids)
            {
                // TODO: add code to implement circular grids
                // Create GridSystem in RAM for each unique centerpt of circGrids  
            }

            foreach (Grid skGrid in skewGrids)
            {
                // TODO: add code to implement skewed grids
                // Create GridSystem in RAM for each unique angle of skewGrids
            }

            //get the ID of the gridsystem
            int gridSystemID = ramGridSystemXY.lUID;

            //Cycle through floortypes, access the existing floortype/story, place grids on those stories
            for (int i = 0; i < ramFloorTypes.GetCount(); i++)
            {
                IFloorType ramFloorType = ramFloorTypes.GetAt(i);
                DAArray gsID = ramFloorType.GetGridSystemIDArray();
                gsID.Add(ramGridSystemXY.lUID, 0);
                ramFloorType.SetGridSystemIDArray(gsID);
            }

            //Save file
            RAMDataAccIDBIO.SaveDatabase();
            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            return true;
        }

        /***************************************************/

        private void CreateElementError(string elemType, string elemName)
        {
            Engine.Reflection.Compute.RecordError("Failed to create the element of type " + elemType + ", with id: " + elemName);
        }

        /***************************************************/

        private void CreatePropertyError(string failedProperty, string elemType, string elemName)
        {
            CreatePropertyEvent(failedProperty, elemType, elemName, oM.Reflection.Debugging.EventType.Error);
        }

        /***************************************************/

        private void CreatePropertyWarning(string failedProperty, string elemType, string elemName)
        {
            CreatePropertyEvent(failedProperty, elemType, elemName, oM.Reflection.Debugging.EventType.Warning);
        }

        /***************************************************/

        private void CreatePropertyEvent(string failedProperty, string elemType, string elemName, oM.Reflection.Debugging.EventType eventType)
        {
            Engine.Reflection.Compute.RecordEvent("Failed to set property " + failedProperty + " for the " + elemType + " with id: " + elemName, eventType);
        }

        /***************************************************/
    }
}
