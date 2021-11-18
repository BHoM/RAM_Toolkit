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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Geometry.SettingOut;
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
        private bool CreateCollection(IEnumerable<ISurfaceProperty> srfProps)
        {
            foreach (ISurfaceProperty srfProp in srfProps)
            {
                RAMId RAMId = new RAMId();
                if (srfProp is Ribbed)
                {
                    Ribbed compProp = (Ribbed)srfProp;
                    ICompDeckProp ramProp;

                    //Validity check for ribbed properties thickness above flutes
                    if (compProp.Thickness < 2.0.FromInch())
                    {
                        Engine.Reflection.Compute.RecordError("Deck property " + srfProp.Name + " has an invalid thickness. Thickness was automatically changed to 2 inches in order to ensure required shear stud engagement per RAM.");
                        compProp.Thickness = 2.0.FromInch();
                    }
                    ICompDeckProps compDeckProps = m_Model.GetCompositeDeckProps();
                    try
                    {
                        ramProp = compDeckProps.Add2(compProp.Name, Engine.Reflection.Query.PropertyValue(compProp, "DeckProfileName").ToString(), compProp.Thickness.ToInch(), compProp.TotalDepth.ToInch() - 1.5);
                    }
                    catch
                    {
                        Engine.Reflection.Compute.RecordWarning("Deck label for surface property " + srfProp.Name + " not found or invalid for specified thickness. Using default deck profile. Please provide a valid deck name from the RAM Deck Table as a property on the SurfaceProperty named DeckProfileName.");
                        ramProp = compDeckProps.Add2(compProp.Name, "ASC 3W", compProp.Thickness.ToInch(), 4.5);
                    }
                    RAMId.Id = ramProp.lUID;
                }
                else if (srfProp is ConstantThickness && !(srfProp.Material is Steel))
                {
                    ConstantThickness prop = (ConstantThickness)srfProp;
                    if (prop.PanelType != PanelType.Wall)  //Wall surface properties are created on a per wall element basis
                    {
                        IConcSlabProps concSlabProps = m_Model.GetConcreteSlabProps();
                        IConcSlabProp ramProp;
                        ramProp = concSlabProps.Add(prop.Name, prop.Thickness.ToInch(), prop.Material.Density.ToPoundPerCubicFoot());
                        RAMId.Id = ramProp.lUID;
                    }
                }
                else if (srfProp is ConstantThickness && srfProp.Material is Steel)
                {
                    ConstantThickness prop = (ConstantThickness)srfProp;
                    if (prop.PanelType != PanelType.Wall)  //Wall surface properties are created on a per wall element basis
                    {
                        INonCompDeckProps nonCompDeckProps = m_Model.GetNonCompDeckProps();
                        INonCompDeckProp ramProp;

                        ramProp = nonCompDeckProps.Add(prop.Name);
                        ramProp.dEffectiveThickness = prop.Thickness.ToInch();
                        RAMId.Id = ramProp.lUID;
                    }
                }
                srfProp.SetAdapterId(RAMId);
            }

            //Save file
            m_IDBIO.SaveDatabase();

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
                if (Math.Abs(panelNormZ) < 0.707) // check normal against 45 degree slope
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
                RAMId RAMId = new RAMId();
                string name = panel.Name;
                PolyCurve outlineExternal = panel.OutlineCurve();
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
                        ramSlabEdges.Add(startPt.X.ToInch(), startPt.Y.ToInch(), endPt.X.ToInch(), endPt.Y.ToInch(), 0);
                    }

                    List<Opening> panelOpenings = panel.Openings;

                    foreach (Opening opening in panelOpenings)
                    {
                        PolyCurve outlineOpening = opening.OutlineCurve();
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
                            ramOpeningEdges.Add(startPt.X.ToInch(), startPt.Y.ToInch(), endPt.X.ToInch(), endPt.Y.ToInch(), 0);
                        }
                    }

                    // Create Deck 
                    List<Point> ctrlPoints = cwOutline.ControlPoints();

                    if (ctrlPoints.First() != ctrlPoints.Last())
                    {
                        ctrlPoints.Add(ctrlPoints.Last().DeepClone());
                    }

                    ISurfaceProperty srfProp = panel.Property;
                    int deckProplUID = GetAdapterId<int>(srfProp);

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
                    double thickness = 0.2; // default thickness
                    if (wallPanel.Property is ConstantThickness)
                    {
                        ConstantThickness prop = (ConstantThickness)wallPanel.Property;
                        thickness = prop.Thickness;
                    }

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Spatial.Query.OutlineCurve(wallPanel);
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
                        if (Math.Round(wallMax.Z.ToInch(), 0) >= ramStory.dElevation && Math.Round(wallMin.Z.ToInch(), 0) < ramStory.dElevation)
                        {
                            ramFloorType = ramStory.GetFloorType();

                            //Get ILayoutWalls of FloorType and add wall
                            ILayoutWalls ramLayoutWalls = ramFloorType.GetLayoutWalls();
                            ILayoutWall ramLayoutWall = ramLayoutWalls.Add(EMATERIALTYPES.EWallPropConcreteMat, wallMin.X.ToInch(), wallMin.Y.ToInch(), 0, 0, wallMax.X.ToInch(), wallMax.Y.ToInch(), 0, 0, thickness.ToInch());

                            //Set lateral
                            ramLayoutWall.eFramingType = EFRAMETYPE.MemberIsLateral;

                            IWalls ramWalls = ramLayoutWall.GetAssociatedStoryWalls();
                            IWall ramWall = ramWalls.GetAt(0);

                            // Find opening location, width, and height from outline and apply                      
                            foreach (Opening open in wallPanel.Openings)
                            {
                                PolyCurve openOutline = open.OutlineCurve();
                                BoundingBox openBounds = BH.Engine.Geometry.Query.Bounds(openOutline);
                                Point openMin = openBounds.Min;
                                Point openMax = openBounds.Max;

                                if ((openMin.Z.ToInch() >= ramStory.dElevation - ramStory.dFlrHeight) && (openMin.Z.ToInch() < ramStory.dElevation))
                                {
                                    IFinalWallOpenings ramWallOpenings = ramWall.GetFinalOpenings();

                                    int openOverlapCount = 0;

                                    for (int j = 0; i < ramWallOpenings.GetCount(); j++)
                                    {
                                        IFinalWallOpening testOpen = ramWallOpenings.GetAt(j);
                                        IPoints openingPts = testOpen.GetOpeningVertices();

                                        //Re-add first point to close Polygon
                                        IPoint firstOPt = openingPts.GetAt(0);
                                        SCoordinate firstOCoord = new SCoordinate();
                                        firstOPt.GetCoordinate(ref firstOCoord);
                                        openingPts.Add(firstOCoord);

                                        Polyline wallOpeningOutline = openingPts.ToPolyline();
                                        List<Point> intPts = wallOpeningOutline.ICurveIntersections(openOutline);
                                        if (wallOpeningOutline.IsContaining(openOutline) || openOutline.IsContaining(wallOpeningOutline) || intPts.Count > 0)
                                        { openOverlapCount += 1; }
                                    }

                                    if (openOverlapCount == 0)
                                    {
                                        //Get opening on wall extents
                                        if (!(outline.IsContaining(openOutline, false)))
                                        {
                                            openOutline = outline.BooleanIntersection(openOutline)[0];
                                            Engine.Reflection.Compute.RecordWarning("Panel " + name + " opening intersects wall boundary. Boolean intersection was used to get opening extents on panel.");
                                        }

                                        Point closestOpenPt = BH.Engine.Geometry.Query.ClosestPoint(wallMin, openOutline.ControlPoints());
                                        double distX = Math.Sqrt(Math.Pow(closestOpenPt.X - wallMin.X, 2) + Math.Pow(closestOpenPt.Y - wallMin.Y, 2));
                                        double distZinch = openBounds.Min.Z.ToInch() - (ramStory.dElevation - ramStory.dFlrHeight);
                                        double openWidth = Math.Sqrt(Math.Pow(openBounds.Max.X - openBounds.Min.X, 2) + Math.Pow(openBounds.Max.Y - openBounds.Min.Y, 2));
                                        double openHt = openBounds.Max.Z - openBounds.Min.Z;

                                        //Add opening to RAM
                                        IRawWallOpenings ramRawWallOpenings = ramWall.GetRawOpenings();
                                        ramRawWallOpenings.Add(EDA_MEMBER_LOC.eBottomStart, distX.ToInch(), distZinch, openWidth.ToInch(), openHt.ToInch());
                                    }
                                }
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
    }
}