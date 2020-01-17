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
using BH.oM.Geometry.SettingOut;
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
using BH.Engine.Adapters.RAM;
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Structure.Loads;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {

        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
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

            //Get the stories in the model
            IStories ramStories = m_Model.GetStories();

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
                ILayoutBeam ramBeam;

                try
                {
                    IStory barStory = bar.GetStory(StructuralUsage1D.Beam, ramStories);

                    IFloorType ramFloorType = barStory.GetFloorType();
                    ILayoutBeams ramBeams = ramFloorType.GetLayoutBeams();

                    double zStart = bar.StartNode.Position().Z - barStory.dElevation;
                    double zEnd = bar.EndNode.Position().Z - barStory.dElevation;

                    //  Get critical cant values
                    object isStubCant;
                    bar.CustomData.TryGetValue("IsStubCantilever", out isStubCant);
                    isStubCant = isStubCant == null ? "" : isStubCant.ToString();
                    object startCantObj;
                    object endCantObj;
                    bar.CustomData.TryGetValue("StartCantilever", out startCantObj);
                    bar.CustomData.TryGetValue("EndCantilever", out endCantObj);
                    double startCant, endCant;
                    double.TryParse(startCantObj == null ? "" : startCantObj.ToString(), out startCant);
                    double.TryParse(endCantObj == null ? "" : endCantObj.ToString(), out endCant);

                    if (isStubCant.Equals("True") || isStubCant.Equals("1")) //Check bool per RAM or GH preferred boolean context
                    {
                        Point startPt, endPt;
                        if (startCant > 0) // Ensure startPt corresponds with support point
                        {
                            startPt = bar.EndNode.Position();
                            endPt = bar.StartNode.Position();
                        }
                        else
                        {
                            startPt = bar.StartNode.Position();
                            endPt = bar.EndNode.Position();
                        }


                        double xStart = startPt.X;
                        double yStart = startPt.Y;
                        double xEnd = endPt.X;
                        double yEnd = endPt.Y;

                        ramBeam = ramBeams.AddStubCantilever(bar.SectionProperty.Material.ToRAM(), xStart, yStart, 0, xEnd, yEnd, 0); // No Z offsets, beams flat on closest story
                    }
                    else
                    {
                        //  Get support points
                        Vector barDir = bar.Tangent(true);
                        Point startSupPt = BH.Engine.Geometry.Modify.Translate(bar.StartNode.Position(), barDir * startCant);
                        Point endSupPt = BH.Engine.Geometry.Modify.Translate(bar.EndNode.Position(), -barDir * endCant);

                        ramBeam = ramBeams.Add(bar.SectionProperty.Material.ToRAM(), startSupPt.X, startSupPt.Y, 0, endSupPt.X, endSupPt.Y, 0); // No Z offsets, beams flat on closest story
                        if (startSupPt.X < endSupPt.X || (startSupPt.X == endSupPt.X && startSupPt.Y>endSupPt.Y))
                        {
                            ramBeam.dStartCantilever = startCant;
                            ramBeam.dEndCantilever = endCant;
                        }
                        else
                        {
                            ramBeam.dStartCantilever = endCant;
                            ramBeam.dEndCantilever = startCant;
                        }
                    }

                    // Add warning to report distance of snapping to level as required for RAM
                    if (zStart != 0 || zEnd != 0)
                    {Engine.Reflection.Compute.RecordWarning("Bar " + name + " snapped to level " + barStory.strLabel + ". Bar moved " + Math.Round(zStart,2).ToString() + " at start and " + Math.Round(zEnd,2).ToString() + " at end."); }

                    IBeams beamsOnStory = barStory.GetBeams();
                    IBeam beam = beamsOnStory.Get(ramBeam.lUID);
                    beam.strSectionLabel = bar.SectionProperty.Name;
                    // beam.EAnalyzeFlag = EAnalyzeFlag.eAnalyze; deprecated in API 
                }
                catch
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
                    object isHanging;
                    bar.CustomData.TryGetValue("IsHangingColumn", out isHanging);
                    isHanging = isHanging == null ? "" : isHanging.ToString();

                    if (isHanging.Equals("True") || isHanging.Equals("1")) //Check bool per RAM or GH preferred boolean context
                    {
                        ramColumn = ramColumns.Add3(bar.SectionProperty.Material.ToRAM(), xBtm, yBtm, xTop, yTop, 0, 0, 1); //No Z offsets, cols start and end at stories
                    }  
                    else if (bar.IsVertical())
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
                catch
                {
                    CreateElementError("bar", name);
                }
            }

            //Save file
            m_IDBIO.SaveDatabase();

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

        private bool CreateCollection(IEnumerable<ISurfaceProperty> srfProps)
        {
            foreach (ISurfaceProperty srfProp in srfProps)
            {
                try
                {
                    ICompDeckProps ramCompDeckProps = m_Model.GetCompositeDeckProps();

                    int existingProp = 0;

                    //Check if load set already exists
                    for (int i = 0; i < ramCompDeckProps.GetCount(); i++)
                    {
                        ICompDeckProp ramProp = ramCompDeckProps.GetAt(i);
                        if (ramProp.strLabel == srfProp.Name)
                        {
                            existingProp = ramProp.lUID;
                        }
                    }

                    if (existingProp == 0)
                    {
                        //Add the Deck prop if it does not already exist
                        string deckName = srfProp.Name;
                        string deckType = "ASC 3W";
                        double thickness = 3; // above deck
                        double studLength = 4.5;
                       
                        ICompDeckProp ramProp = ramCompDeckProps.Add2(deckName, deckType, thickness, studLength);

                        //Set the custom data to return if created
                        srfProp.CustomData[AdapterIdName] = ramProp.lUID;
                    }
                    else
                    {
                        //Set the custom data to return if already existing
                        srfProp.CustomData[AdapterIdName] = existingProp;
                    }
                }
                catch
                {
                    CreateElementError("Panel Deck Property", srfProp.Name);
                }
            }

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

            ramStories = m_Model.GetStories();

            #region Create Floors

            // Cycle through floors and create on story
            foreach (Panel panel in floors)
            {
                string name = panel.Name;
                PolyCurve outlineExternal = panel.Outline();
                ramStory = panel.GetStory(ramStories);
                ramFloorType = ramStory.GetFloorType();

                try
                {
                    // Set slab edges on FloorType in RAM for external edges
                    ISlabEdges ramSlabEdges = ramFloorType.GetAllSlabEdges();
                    ISlabEdges ramOpeningEdges = ramFloorType.GetAllSlabOpenings();

                    // Get external and internal edges of floor panel
                    List<PolyCurve> panelOutlines = new List<PolyCurve>();
                    List<PolyCurve> openingOutlines = new List<PolyCurve>();

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

                    List<Opening> panelOpenings = panel.Openings;

                    foreach (Opening opening in panelOpenings)
                    {
                        PolyCurve outlineOpening = opening.Outline();
                        openingOutlines.Add(outlineOpening);
                    }

                    foreach (PolyCurve outline in openingOutlines)
                    {
                        // RAM requires edges clockwise, flip if counterclockwise
                        PolyCurve cwOpenOutline = (outline.IsClockwise(zDown) == false) ? outline.Flip() : outline;

                        if (!(outlineExternal.IsContaining(cwOpenOutline, false)))
                        {
                            cwOpenOutline = outlineExternal.BooleanIntersection(cwOpenOutline)[0];
                            Engine.Reflection.Compute.RecordWarning("Panel " + name + " opening intersects floor boundary. Boolean intersection was used to get opening extents on panel, confirm opening extents in RAM.");
                        }

                        List<ICurve> openEdgeCrvs = cwOpenOutline.Curves;

                        foreach (ICurve crv in openEdgeCrvs)
                        {
                            Point startPt = crv.IStartPoint();
                            Point endPt = crv.IEndPoint();
                            ramOpeningEdges.Add(startPt.X, startPt.Y, endPt.X, endPt.Y, 0);
                        }
                    }

                    // Create Deck 
                    List<Point> ctrlPoints = cwOutline.ControlPoints();

                    if (ctrlPoints.First() != ctrlPoints.Last())
                    {
                        ctrlPoints.Add(ctrlPoints.Last().Clone());
                    }

                    int deckProplUID = (int)panel.Property.CustomData[AdapterIdName];

                    //Add decks, then set deck points per outline
                    IDecks ramDecks = ramFloorType.GetDecks();
                    IDeck ramDeck = ramDecks.Add(deckProplUID, ctrlPoints.Count);

                    IPoints ramPoints = ramDeck.GetPoints();

                    // Create list of SCoordinates for floor outlines
                    List<SCoordinate> cornersExt = new List<SCoordinate>();

                    foreach (Point point in ctrlPoints)
                    {
                        SCoordinate cornerExt = point.ToRAM();
                        cornersExt.Add(cornerExt);
                    }

                    for (int k = 0; k < cornersExt.Count; k++)
                    {
                        ramPoints.Delete(k);
                        ramPoints.InsertAt(k, cornersExt[k]);
                    }

                    ramDeck.SetPoints(ramPoints);

                    // Add warning to report floors flattened to level as required for RAM
                    if (Math.Abs(panel.Normal().Z) < 1)
                    { Engine.Reflection.Compute.RecordWarning("Panel " + name + " snapped to level " + ramStory.strLabel + "."); }
                }
                catch
                {
                    CreateElementError("panel", name);
                }
            }
            #endregion

            #region Create Walls
            //Cycle through walls; if wall crosses level place at level
            foreach (Panel wallPanel in wallPanels)
                {
                string name = wallPanel.Name;

                try
                {
                    double thickness = 6; // default thickness
                    if (wallPanel.Property is ConstantThickness)
                    {
                        ConstantThickness prop = (ConstantThickness)wallPanel.Property;
                        thickness = prop.Thickness;
                    }

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Structure.Query.Outline(wallPanel);
                    List<Point> wallPts = outline.DiscontinuityPoints();
                    List<Point> sortedWallPts = wallPts.OrderBy(p => p.X).ToList();
                    Point leftPt = sortedWallPts.First();
                    Point rtPt = sortedWallPts.Last();
                    bool downToRight = leftPt.Y > rtPt.Y;                   

                    BoundingBox wallBounds = BH.Engine.Geometry.Query.Bounds(outline);
                    Point wallMin = wallBounds.Min;
                    Point wallMax = wallBounds.Max;
                    double tempY = wallMin.Y;

                    wallMin.Y = downToRight ? wallMax.Y : wallMin.Y;
                    wallMax.Y = downToRight ? tempY : wallMax.Y;

                    for (int i = 0; i < ramStories.GetCount(); i++)
                    {
                        ramStory = ramStories.GetAt(i);
                        // If wall crosses level, add wall to ILayoutWalls for that level
                        if (Math.Round(wallMax.Z, 0) >= ramStory.dElevation && Math.Round(wallMin.Z, 0) < ramStory.dElevation)
                        {
                            ramFloorType = ramStory.GetFloorType();
                            
                            //Get ILayoutWalls of FloorType and add wall
                            ILayoutWalls ramLayoutWalls = ramFloorType.GetLayoutWalls();
                            ILayoutWall ramLayoutWall = ramLayoutWalls.Add(EMATERIALTYPES.EWallPropConcreteMat, wallMin.X, wallMin.Y, 0, 0, wallMax.X, wallMax.Y, 0, 0, thickness);

                            //Set lateral
                            ramLayoutWall.eFramingType = EFRAMETYPE.MemberIsLateral;

                            IWalls ramWalls = ramLayoutWall.GetAssociatedStoryWalls();
                            IWall ramWall = ramWalls.GetAt(0);

                            // Find opening location, width, and height from outline and apply                      
                            foreach (Opening open in wallPanel.Openings)
                            {

                                PolyCurve openOutline = open.Outline();

                                //Get opening on wall extents
                                if (!(outline.IsContaining(openOutline, false)))
                                {
                                    openOutline = outline.BooleanIntersection(openOutline)[0];
                                    Engine.Reflection.Compute.RecordWarning("Panel " + name + " opening intersects wall boundary. Boolean intersection was used to get opening extents on panel.");
                                }

                                BoundingBox openBounds = BH.Engine.Geometry.Query.Bounds(openOutline);
                                Point closestOpenPt = BH.Engine.Geometry.Query.ClosestPoint(wallMin, openOutline.ControlPoints());
                                double distX = Math.Sqrt(Math.Pow(closestOpenPt.X - wallMin.X, 2) + Math.Pow(closestOpenPt.Y - wallMin.Y, 2));
                                double distZ = openBounds.Min.Z - (ramStory.dElevation - ramStory.dFlrHeight);
                                double openWidth = Math.Sqrt(Math.Pow(openBounds.Max.X - openBounds.Min.X, 2) + Math.Pow(openBounds.Max.Y - openBounds.Min.Y, 2));
                                double openHt = openBounds.Max.Z - openBounds.Min.Z;

                                //Add opening to RAM
                                IRawWallOpenings ramWallOpenings = ramWall.GetRawOpenings();
                                ramWallOpenings.Add(EDA_MEMBER_LOC.eBottomStart, distX, distZ, openWidth, openHt);
                            }
                        }
                    }
            }
                catch
            {
                CreateElementError("panel", name);
            }
        }
            #endregion

            //Save file
            m_IDBIO.SaveDatabase();

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

                //Create floor type at each level
                for (int i = 0; i < sortedBhomLevels.Count(); i++)
                {
                    Level level = sortedBhomLevels.ElementAt(i);

                    // Get elevations and skip if level elevation already in RAM
                    ramStories = m_Model.GetStories();
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
                        ramFloorTypes = m_Model.GetFloorTypes();
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
                m_IDBIO.SaveDatabase();

            }

            return true;

        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Grid> bhomGrid)
        {
            // Register GridSystems
            IGridSystems ramGridSystems = m_Model.GetGridSystems();

            // Register FloorTypes
            IFloorTypes ramFloorTypes = m_Model.GetFloorTypes();

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
            //IGridSystem ramGridSystemRad = null;
            //IGridSystem ramGridSystemSk = null;
            IModelGrids ramModelGridsXY = null;
            //IModelGrids ramModelGridsRad = null;
            //IModelGrids ramModelGridsSk = null;


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
            m_IDBIO.SaveDatabase();

            return true;
        }

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

                        ramLoadSet.dConstDeadLoad = loadSet.Loads[ELoadCaseType.ConstructionDeadLCa.ToString()];
                        ramLoadSet.dConstLiveLoad = loadSet.Loads[ELoadCaseType.ConstructionLiveLCa.ToString()];
                        ramLoadSet.dDeadLoad = loadSet.Loads[ELoadCaseType.DeadLCa.ToString()];
                        ramLoadSet.dMassDeadLoad = loadSet.Loads[ELoadCaseType.MassDeadLCa.ToString()];
                        ramLoadSet.dPartitionLoad = loadSet.Loads[ELoadCaseType.PartitionLCa.ToString()];

                        //Check which live load case has been applied, to set load type. Not currently checking if more than one has been set.
                        Engine.Reflection.Compute.RecordNote("If more than one live load has been set, only the first one will be applied");

                        if (loadSet.Loads.ContainsKey(ELoadCaseType.LiveLCa.ToString()))
                        {
                            ramLoadSet.eLiveLoadType = ELoadCaseType.LiveReducibleLCa;
                            ramLoadSet.dLiveLoad = loadSet.Loads[ELoadCaseType.LiveReducibleLCa.ToString()];
                        }
                        else if (loadSet.Loads.ContainsKey(ELoadCaseType.LiveStorageLCa.ToString()))
                        {
                            ramLoadSet.eLiveLoadType = ELoadCaseType.LiveStorageLCa;
                            ramLoadSet.dLiveLoad = loadSet.Loads[ELoadCaseType.LiveStorageLCa.ToString()];
                        }
                        else if (loadSet.Loads.ContainsKey(ELoadCaseType.LiveUnReducibleLCa.ToString()))
                        {
                            ramLoadSet.eLiveLoadType = ELoadCaseType.LiveUnReducibleLCa;
                            ramLoadSet.dLiveLoad = loadSet.Loads[ELoadCaseType.LiveUnReducibleLCa.ToString()];
                        }
                        else if (loadSet.Loads.ContainsKey(ELoadCaseType.LiveRoofLCa.ToString()))
                        {
                            ramLoadSet.eLiveLoadType = ELoadCaseType.LiveRoofLCa;
                            ramLoadSet.dLiveLoad = loadSet.Loads[ELoadCaseType.LiveRoofLCa.ToString()];
                        }
                        //Set the custom data to return if created
                        loadSet.CustomData[AdapterIdName] = ramLoadSet.lUID;
                    }
                    else
                    {
                        //Set the custom data to return if already existing
                        loadSet.CustomData[AdapterIdName] = existingLoadPropSetID;
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

        private bool CreateCollection(IEnumerable<ContourLoadSet> loads)
        {
            foreach (ContourLoadSet load in loads)
            {
                try
                {
                    //Ensure points describe a closed polyline
                    List<Point> loadPoints = load.Contour.ControlPoints();
                    if (loadPoints.First() != loadPoints.Last())
                    {
                        loadPoints.Add(loadPoints.Last().Clone());
                    }

                    //Find the layout to apply to
                    IStories ramStories = m_Model.GetStories();
                    IStory loadStory = loadPoints.First().GetStory(ramStories);
                    double storyElev = loadStory.dElevation;
                    IFloorType floorType = loadStory.GetFloorType();

                    ISurfaceLoadSets floorLoads = floorType.GetSurfaceLoadSets2();
                    int nextId = floorLoads.GetCount();
                    ISurfaceLoadSet ramLoad = floorLoads.Add(nextId, loadPoints.Count());
                    IPoints verticePoints = ramLoad.GetPoints();

                    List<SCoordinate> checkList = new List<SCoordinate>();
                    SCoordinate verticeCoord;

                    for (int i = 0; i < loadPoints.Count(); i++)
                    {
                        verticeCoord = loadPoints[i].ToRAM();
                        verticePoints.Delete(i);
                        verticePoints.InsertAt2(i, verticeCoord.dXLoc, verticeCoord.dYLoc, 0);
                        checkList.Add(verticeCoord);
                    }
                    ramLoad.SetPoints(verticePoints);

                    ramLoad.lPropertySetUID = (int)load.UniformLoadSet.CustomData[AdapterIdName];
                }

                catch
                {
                    CreateElementError("UniformLoadSet", load.Name);
                }
            }



            //Save file
            m_IDBIO.SaveDatabase();

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
