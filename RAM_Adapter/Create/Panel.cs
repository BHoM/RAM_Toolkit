/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

        private bool CreateCollection(IEnumerable<Panel> bhomPanels)
        {
            //Code for creating a collection of floors and walls in the software

            // Split walls and floors and get all elevations
            IEnumerable<Panel> floors = bhomPanels.Where(x => Math.Abs(x.Normal().Z) >= 0.707);
            IEnumerable<Panel> walls = bhomPanels.Where(x => Math.Abs(x.Normal().Z) < 0.707);

            IStories ramStories = m_Model.GetStories();

            #region Create Floors

            for (int i = 0; i < ramStories.GetCount(); i++)
            {
                IStory ramStory = ramStories.GetAt(i);
                IFloorType ramFloorType = ramStory.GetFloorType();

                IEnumerable<Panel> storyFloors = floors.Where(x => x.GetStory(ramStories).lUID == ramStory.lUID);

                ramStory.Equals(ramStory);

                //RAM can't handle adjoining slab edges, so we merge them.
                List<Polyline> outlineCurves = storyFloors.Select(x => x.OutlineCurve().ToPolyline()).ToList();
                List<List<Polyline>> outlineCurvesDistributed = outlineCurves.BooleanUnion().DistributeOutlines();

                List<Polyline> outlines = outlineCurvesDistributed.Select(x => x.First()).ToList();

                //Flip all outlines so that they are clockwise.
                outlines = outlines.Select(pl => pl.IsClockwise(Vector.ZAxis) ? pl : pl.Flip()).ToList();

                //Write slab edges to RAM
                ISlabEdges ramSlabEdges = ramFloorType.GetAllSlabEdges();
                foreach (Line edge in outlines.SelectMany(x => x.SubParts()))
                {
                    SCoordinate startPt = edge.IStartPoint().ToRAM();
                    SCoordinate endPt = edge.IEndPoint().ToRAM();
                    ramSlabEdges.Add(startPt.dXLoc, startPt.dYLoc, endPt.dXLoc, endPt.dYLoc, 0);
                }

                //Add the openings
                IEnumerable<Opening> panelOpenings = storyFloors.SelectMany(x => x.Openings);
                List<Polyline> openingOutlines = panelOpenings.Select(x => x.OutlineCurve().ToPolyline()).ToList();

                //Add the new openings resulting from the external edges boolean.
                openingOutlines.AddRange(outlineCurvesDistributed.SelectMany(x => x.Skip(1)).ToList());

                //Boolean all openings, discard any resulting internal openings.
                openingOutlines = openingOutlines.BooleanUnion().DistributeOutlines().Select(x => x.First()).ToList();

                //Flip all outlines so that they are clockwise.
                openingOutlines = openingOutlines.Select(pl => pl.IsClockwise(Vector.ZAxis) ? pl : pl.Flip()).ToList();

                ISlabEdges ramOpeningEdges = ramFloorType.GetAllSlabOpenings();
                foreach (Line edge in openingOutlines.SelectMany(x => x.SubParts()))
                {
                    SCoordinate startPt = edge.IStartPoint().ToRAM();
                    SCoordinate endPt = edge.IEndPoint().ToRAM();
                    ramOpeningEdges.Add(startPt.dXLoc, startPt.dYLoc, endPt.dXLoc, endPt.dYLoc, 0);
                }

                // Create all the deck assignments (these can be adjoining)
                foreach (Panel bhFloorPanel in storyFloors)
                {
                    string name = bhFloorPanel.Name;

                    PolyCurve outlineExternal = bhFloorPanel.OutlineCurve();

                    // RAM requires edges clockwise, flip if counterclockwise
                    outlineExternal = outlineExternal.IsClockwise(Vector.ZAxis) ? outlineExternal : outlineExternal.Flip();

                    List<ICurve> edgeCrvs = outlineExternal.Curves;

                    List<Point> ctrlPoints = outlineExternal.ControlPoints();

                    if (!ctrlPoints.First().IsEqual(ctrlPoints.Last()))
                    {
                        ctrlPoints.Add(ctrlPoints.Last().DeepClone());
                    }

                    ISurfaceProperty srfProp = bhFloorPanel.Property;
                    int deckProplUID = GetAdapterId<int>(srfProp);

                    if (deckProplUID != 0)
                    {

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
                        if (Math.Abs(bhFloorPanel.Normal().Z) < 1)
                        { Engine.Base.Compute.RecordWarning("Panel " + name + " snapped to level " + ramStory.strLabel + "."); }

                        // Add an adapter ID to the incoming panel.
                        RAMId id = new RAMId() { Id = ramDeck.lUID };
                        bhFloorPanel.SetAdapterId(id);
                    }
                    else
                    {
                        bhFloorPanel.SetAdapterId(new RAMId());
                        Engine.Base.Compute.RecordError($"Panel {name} has a section property with no AdapterID, so the deck could not be assigned in RAM.");
                    }
                }
            }
            #endregion

            #region Create Walls

            //Cycle through walls; if wall crosses level place at level
            foreach (Panel bhWallPanel in walls)
            {
                string name = bhWallPanel.Name;

                try
                {
                    double thickness = 0.2; // default thickness
                    if (bhWallPanel.Property is ConstantThickness)
                    {
                        ConstantThickness prop = (ConstantThickness)bhWallPanel.Property;
                        thickness = prop.Thickness;
                    }

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Spatial.Query.OutlineCurve(bhWallPanel);
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
                        IStory ramStory = ramStories.GetAt(i);
                        // If wall crosses level, add wall to ILayoutWalls for that level
                        if (Math.Round(wallMax.Z.ToInch(), 0) >= ramStory.dElevation && Math.Round(wallMin.Z.ToInch(), 0) < ramStory.dElevation)
                        {
                            IFloorType ramFloorType = ramStory.GetFloorType();

                            //Get ILayoutWalls of FloorType and add wall
                            ILayoutWalls ramLayoutWalls = ramFloorType.GetLayoutWalls();
                            ILayoutWall ramLayoutWall = ramLayoutWalls.Add(EMATERIALTYPES.EWallPropConcreteMat, wallMin.X.ToInch(), wallMin.Y.ToInch(), 0, 0, wallMax.X.ToInch(), wallMax.Y.ToInch(), 0, 0, thickness.ToInch());

                            //Set lateral
                            ramLayoutWall.eFramingType = EFRAMETYPE.MemberIsLateral;

                            IWalls ramWalls = ramLayoutWall.GetAssociatedStoryWalls();
                            IWall ramWall = ramWalls.GetAt(0);

                            // Find opening location, width, and height from outline and apply                      
                            foreach (Opening open in bhWallPanel.Openings)
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
                                            Engine.Base.Compute.RecordWarning("Panel " + name + " opening intersects wall boundary. Boolean intersection was used to get opening extents on panel.");
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

                            // Add an adapter ID to the incoming panel.
                            RAMId id = new RAMId() { Id = ramWall.lUID };
                            bhWallPanel.SetAdapterId(id);
                        }

                    }
                }
                catch
                {
                    bhWallPanel.SetAdapterId(new RAMId());
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




