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
using System.Collections.Generic;
using System.Linq;
using BH.oM.Geometry;
using BH.oM.Physical;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Architecture.Elements;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Loads;
using BH.oM.Structure.Constraints;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.Results;
using RAMDATAACCESSLib;

namespace BH.Engine.RAM
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        /// <summary>
        //Add methods for converting to BHoM from the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static Node ToBHoM(this RAMNode node)
        //{
        //    //Insert code for conversion
        //}
        /// <summary>
        /***************************************************/

        public static List<double> ToLevelElevations(this IModel IModel)
        {
            List<double> RAMLevelHeights = new List<double>();

            //Get existing levels
            List<string> FloorTypeNames = new List<string>();
            List<string> StoryNames = new List<string>();
            double StoryElevation;
            IFloorTypes IFloorTypes = IModel.GetFloorTypes();
            IStories IStories = IModel.GetStories();

            double storyCount = IStories.GetCount();

            for (int i = 0; i < storyCount; i++)
            {
                StoryElevation = IStories.GetAt(i).dElevation;
                RAMLevelHeights.Add(StoryElevation);
            }
            return RAMLevelHeights;
        }

        /***************************************************/
        
        public static Polyline ToPolyline(this IPoints IPoints)
        {
            List<Point> controlPts = new List<Point>();
            SCoordinate SCoordPt = new SCoordinate();

            for (int i = 0; i < IPoints.GetCount(); i++)
            {
                //Get Polyline Pts
                IPoint IPoint = IPoints.GetAt(i);
                IPoint.GetCoordinate(ref SCoordPt);
                Point controlPt = new BH.oM.Geometry.Point() { X = SCoordPt.dXLoc, Y = SCoordPt.dYLoc, Z = SCoordPt.dZLoc };
                controlPts.Add(controlPt);
            }

            Polyline polyline = new Polyline();
            polyline.ControlPoints = controlPts;
            return polyline;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IBeam RAMBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();
            IMaterialFragment Material = null;

            if (RAMBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
            }
            else if (RAMBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
            }
            else
            {
                Material = Engine.Structure.Create.Steel("Other");
            }
            sectionProperty.Material = Material;
            sectionProperty.Name = RAMBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IColumn RAMBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (RAMBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (RAMBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = RAMBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IVerticalBrace RAMBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (RAMBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (RAMBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = RAMBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IHorizBrace RAMBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (RAMBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (RAMBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = RAMBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IColumn IColumn)
        {

            // Get the column name
            string section = IColumn.strSectionLabel;

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IColumn.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = Structure.Create.Node(new oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc });
            Node endNode = Structure.Create.Node(new oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc });

            //Assign section property per bar
            string sectionName = IColumn.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(IColumn);

            // Create bars with section properties
            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, SectionProperty = sectionProperty, Name = sectionName };

            // Translate RAM Releases to BHoM Releases (in progress; logic not yet complete since it is difficult map Maj/Min axes to global XYZ axes for every member)
            // May be better to just do in custom data, although if we can do this mapping it may be useful
            bhomBar.Release = new BarRelease();
            bhomBar.Release.StartRelease = new Constraint6DOF();
            bhomBar.Release.EndRelease = new Constraint6DOF();
            bhomBar.Release.StartRelease.RotationX = new DOFType();
            bhomBar.Release.EndRelease.RotationX = new DOFType();
            bhomBar.Release.StartRelease.RotationY = new DOFType();
            bhomBar.Release.EndRelease.RotationY = new DOFType();

            if (IColumn.bMajAxisBendFixedTop == 1) { bhomBar.Release.StartRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationX = DOFType.Free; }
            if (IColumn.bMajAxisBendFixedBot == 1) { bhomBar.Release.EndRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationX = DOFType.Free; }
            if (IColumn.bMinAxisBendFixedTop == 1) { bhomBar.Release.StartRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationY = DOFType.Free; }
            if (IColumn.bMinAxisBendFixedBot == 1) { bhomBar.Release.EndRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationY = DOFType.Free; }


            bhomBar.OrientationAngle = 0;

            // Add RAM Unique ID, custom Data
            bhomBar.CustomData["lUID"] = IColumn.lUID;
            bhomBar.CustomData["FrameNumber"] = IColumn.lLabel;
            bhomBar.CustomData["FrameType"] = IColumn.eFramingType.ToString();
            bhomBar.CustomData["Material"] = IColumn.eMaterial.ToString();

            bhomBar.Tags.Add("Column");

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IBeam IBeam, ILayoutBeam ILayoutBeam, double dElevation)
        {

            // Get coordinates from IBeam
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IBeam.GetCoordinates(EBeamCoordLoc.eBeamEnds, ref startPt, ref endPt);
            Node startNode = Structure.Create.Node(new oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc });
            Node endNode = Structure.Create.Node(new oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc });

            //Assign section property per bar
            string sectionName = IBeam.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(IBeam);

            // Create bars with section properties
            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, SectionProperty = sectionProperty, Name = sectionName };

            // Set Properties
            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IBeam.lUID;
            bhomBar.CustomData["FrameNumber"] = IBeam.lLabel;
            bhomBar.CustomData["CantDist"] = IBeam.dEndCantilever.ToString();
            bhomBar.CustomData["FrameType"] = IBeam.eFramingType.ToString();
            bhomBar.CustomData["Material"] = IBeam.eMaterial.ToString();
            bhomBar.Tags.Add("Beam");


            // Get Steel beam results
            ISteelBeamDesignResult Result = IBeam.GetSteelDesignResult();
            DAArray ppalNumStuds = Result.GetNumStudsInSegments();

            int numStudSegments = new int();
            ppalNumStuds.GetSize(ref numStudSegments);
            double Camber = IBeam.dCamber;
            int studCount = 0;

            IAnalyticalResult AnalyticalResult = IBeam.GetAnalyticalResult();
            COMBO_MATERIAL_TYPE Steel_Grav = COMBO_MATERIAL_TYPE.GRAV_STEEL;
            IMemberForces IMemberForces = AnalyticalResult.GetMaximumComboReactions(Steel_Grav);

            //Add studs to custom Data by total stud count only
            for (int i = 0; i < numStudSegments; i++)
            {
                var segStudCount = new object();
                ppalNumStuds.GetAt(i, ref segStudCount);
                string segStudStr = segStudCount.ToString();
                int segStudNum = System.Convert.ToInt16(segStudStr);
                studCount += segStudNum;
                bhomBar.CustomData["Studs"] = studCount.ToString();
            }

            // Add camber to Custom Data
            if (Camber > Double.MinValue)
            {
                bhomBar.CustomData["Camber"] = Camber.ToString();
            }

            // Translate RAM Releases to BHoM Releases (in progress; logic not yet complete since it is difficult map Maj/Min axes to global XYZ axes for every member)
            // May be better to just do in custom data, although if we can do this mapping it may be useful
            bhomBar.Release = new BarRelease();
            bhomBar.Release.StartRelease = new Constraint6DOF();
            bhomBar.Release.EndRelease = new Constraint6DOF();
            bhomBar.Release.StartRelease.RotationX = new DOFType();
            bhomBar.Release.EndRelease.RotationX = new DOFType();
            bhomBar.Release.StartRelease.RotationY = new DOFType();
            bhomBar.Release.EndRelease.RotationY = new DOFType();

            if (IBeam.bMajAxisBendFixedStart == 1) { bhomBar.Release.StartRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationX = DOFType.Free; }
            if (IBeam.bMajAxisBendFixedEnd == 1) { bhomBar.Release.EndRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationX = DOFType.Free; }
            if (IBeam.bMinAxisBendFixedStart == 1) { bhomBar.Release.StartRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationY = DOFType.Free; }
            if (IBeam.bMinAxisBendFixedEnd == 1) { bhomBar.Release.EndRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationY = DOFType.Free; }

            double DCI = Result.dDesignCapacityInteraction;
            double CDI = Result.dCriticalDeflectionInteraction;
            
            // Add DCI and CDI data
            bhomBar.CustomData["DesignCapacityInteraction"] = DCI;
            bhomBar.CustomData["CriticalDeflectionInteraction"] = CDI;

            //// Add reactions to custom data //NOTE: Commented out because it seems to be interfering with "push" (when updating?), sometimes affecting exploding of custom data
            //if (IMemberForces != null)
            //{
            //    IMemberForce Force1 = IMemberForces.GetAt(0);
            //    IMemberForce Force2 = IMemberForces.GetAt(1);

            //    double Axial1 = Force1.dAxial;
            //    double MomentMaj1 = Force1.dMomentMajor;
            //    double ShearMinor1 = Force1.dShearMinor;

            //    double Axial2 = Force2.dAxial;
            //    double MomentMaj2 = Force2.dMomentMajor;
            //    double ShearMinor2 = Force2.dShearMinor;

            //    bhomBar.CustomData.Add("Reac1 Axial", Axial1);
            //    bhomBar.CustomData.Add("Reac1 Moment", MomentMaj1);
            //    bhomBar.CustomData.Add("Reac1 Shear", ShearMinor1);
            //    bhomBar.CustomData.Add("Reac2 Axial", Axial2);
            //    bhomBar.CustomData.Add("Reac2 Moment", MomentMaj2);
            //    bhomBar.CustomData.Add("Reac2 Shear", ShearMinor2);
            //} else
            //{
            //    bhomBar.CustomData.Add("Reac1 Axial", null);
            //    bhomBar.CustomData.Add("Reac1 Moment", null);
            //    bhomBar.CustomData.Add("Reac1 Shear", null);
            //    bhomBar.CustomData.Add("Reac2 Axial", null);
            //    bhomBar.CustomData.Add("Reac2 Moment", null);
            //    bhomBar.CustomData.Add("Reac2 Shear", null);
            //}

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IVerticalBrace IVerticalBrace)
        {

            // Get the column name
            string sectionName = IVerticalBrace.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(IVerticalBrace);

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IVerticalBrace.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = Structure.Create.Node(new oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc });
            Node endNode = Structure.Create.Node(new oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc });


            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, SectionProperty = sectionProperty, Name = sectionName };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IVerticalBrace.lUID;
            bhomBar.CustomData["FrameNumber"] = IVerticalBrace.lLabel;
            bhomBar.CustomData["FrameType"] = IVerticalBrace.eSeismicFrameType.ToString();
            bhomBar.CustomData["Material"] = IVerticalBrace.eMaterial.ToString();
            bhomBar.Tags.Add("VerticalBrace");

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(IHorizBrace IHorizBrace, ILayoutHorizBrace ILayoutHorizBrace, double dElevation)
        {

            string sectionName = IHorizBrace.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(IHorizBrace);

            // Get the start and end pts of every brace
            double StartSupportX = new double();
            double StartSupportY = new double();
            double StartSupportZOffset = new double();
            double EndSupportX = new double();
            double EndSupportY = new double();
            double EndSupportZOffset = new double();
            double StoryZ = dElevation;


            // Get coordinates from ILayout Brace
            ILayoutHorizBrace.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
            Node startNode = Structure.Create.Node(new oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset });
            Node endNode = Structure.Create.Node(new oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset });

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, SectionProperty = sectionProperty, Name = sectionName };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IHorizBrace.lUID;
            bhomBar.CustomData["FrameNumber"] = IHorizBrace.lLabel;
            bhomBar.CustomData["Material"] = IHorizBrace.eMaterial.ToString();
            bhomBar.Tags.Add("HorizontalBrace");

            return bhomBar;
        }

        /***************************************************/

        public static Panel ToBHoMObject(IDeck IDeck, IModel IModel, int IStoryUID)
        {

            //Get panel props
            EDeckType type = IDeck.eDeckPropType;

            //Find polylines of deck in RAM Model

            //get count of deck polygons
            double deckPolyCount = IDeck.GetNumFinalPolygons(IStoryUID);

            //Initial only gets first outline poly for exterior edge, rest for openings
            IPoints pplPoints = IDeck.GetFinalPolygon(IStoryUID, 0);

            //Re-add first point to close Polygon
            IPoint first = pplPoints.GetAt(0);
            SCoordinate firstCoord = new SCoordinate();
            first.GetCoordinate(ref firstCoord);
            pplPoints.Add(firstCoord);

            //Create outline polyline
            Polyline outline = ToPolyline(pplPoints);

            //Create opening outlines
            List<ICurve> openingPLs = new List<ICurve>();

            for (int i = 1; i < deckPolyCount; i++)
            {
                IPoints openingPts = IDeck.GetFinalPolygon(IStoryUID, i);

                //Re-add first point to close Polygon
                IPoint firstOPt = openingPts.GetAt(0);
                SCoordinate firstOCoord = new SCoordinate();
                firstOPt.GetCoordinate(ref firstOCoord);
                openingPts.Add(firstOCoord);

                ICurve openingOutline = ToPolyline(openingPts);

                //Create openings per outline polylines
                openingPLs.Add(openingOutline);
            }

            //Create panel per outline polyline
            List<Opening> bhomOpenings = new List<Opening>();
            Panel bhomPanel = Structure.Create.Panel(outline, bhomOpenings);
            //Create openings per openings polylines
            int numOpenings = openingPLs.Count();

            //Create openings
            for (int i = 0; i < numOpenings; i++)
            {
                Opening bhomOpening = Structure.Create.Opening(openingPLs[i]);
                bhomOpenings.Add(bhomOpening);
            }

            //Create panel and add attributes;
            bhomPanel.Openings = bhomOpenings;

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Floor");

            bhomPanel.Tags = tag;
            bhomPanel.Name = type.ToString();

            //Get all floor props
            ICompDeckProps ICompDeckProps = IModel.GetCompositeDeckProps();
            INonCompDeckProps INonCompDeckProps = IModel.GetNonCompDeckProps();
            IConcSlabProps IConcSlabProps = IModel.GetConcreteSlabProps();

            // Get deck section property
            ConstantThickness deck2DProp = new ConstantThickness();
            double deckThickness = 0;
            string deckLabel = "";
            int deckID = IDeck.lPropID;
            IMaterialFragment Material = null;

            if (type == EDeckType.eDeckType_Composite)
            {
                ICompDeckProp DeckProp = ICompDeckProps.Get(deckID);
                deckThickness = DeckProp.dThickAboveFlutes;
                deckLabel = DeckProp.strLabel + " " + deckThickness.ToString();
                Material = Engine.Structure.Create.Concrete("Composite");

            }
            else if (type == EDeckType.eDeckType_Concrete)
            {
                IConcSlabProp DeckProp = IConcSlabProps.Get(deckID);
                deckThickness = DeckProp.dThickness;
                deckLabel = DeckProp.strLabel;
                Material = Engine.Structure.Create.Concrete("Concrete");
            }
            else if (type == EDeckType.eDeckType_NonComposite)
            {
                INonCompDeckProp DeckProp = INonCompDeckProps.Get(deckID);
                deckThickness = DeckProp.dEffectiveThickness;
                deckLabel = DeckProp.strLabel;
                Material = Engine.Structure.Create.Concrete("NonComposite");
            }

            deck2DProp.Name = deckLabel;
            deck2DProp.Thickness = deckThickness;
            deck2DProp.PanelType = PanelType.Slab;
            deck2DProp.Material = Material;
            bhomPanel.Property = deck2DProp;
            bhomPanel.CustomData["lUID"] = IDeck.lUID;

            return bhomPanel;
        }

        /***************************************************/

        public static Panel ToBHoMObject(this IWall ramWall)
        {

            //Find corner points of wall in RAM model
            SCoordinate TopstartPt = new SCoordinate();
            SCoordinate TopendPt = new SCoordinate();
            SCoordinate BottomstartPt = new SCoordinate();
            SCoordinate BottomendPt = new SCoordinate();

            ramWall.GetEndCoordinates(ref TopstartPt, ref TopendPt, ref BottomstartPt, ref BottomendPt);

            // Create list of points
            List<Point> corners = new List<Point>();
            corners.Add(new Point { X = TopstartPt.dXLoc, Y = TopstartPt.dYLoc, Z = TopstartPt.dZLoc });
            corners.Add(new Point { X = TopendPt.dXLoc, Y = TopendPt.dYLoc, Z = TopendPt.dZLoc });
            corners.Add(new Point { X = BottomendPt.dXLoc, Y = BottomendPt.dYLoc, Z = BottomendPt.dZLoc });
            corners.Add(new Point { X = BottomstartPt.dXLoc, Y = BottomstartPt.dYLoc, Z = BottomstartPt.dZLoc });
            corners.Add(new Point { X = TopstartPt.dXLoc, Y = TopstartPt.dYLoc, Z = TopstartPt.dZLoc });

            // Create outline from corner points
            Polyline outline = new Polyline();
            outline.ControlPoints = corners;

            // Create openings
            IFinalWallOpenings IFinalWallOpenings = ramWall.GetFinalOpenings();

            //Create opening outlines
            List<ICurve> wallOpeningPLs = new List<ICurve>();
            List<Opening> bhomWallOpenings = new List<Opening>();

            for (int i = 0; i < IFinalWallOpenings.GetCount(); i++)
            {
                IFinalWallOpening IFinalWallOpening = IFinalWallOpenings.GetAt(i);
                IPoints openingPts = IFinalWallOpening.GetOpeningVertices();

                //Re-add first point to close Polygon
                IPoint firstOPt = openingPts.GetAt(0);
                SCoordinate firstOCoord = new SCoordinate();
                firstOPt.GetCoordinate(ref firstOCoord);
                openingPts.Add(firstOCoord);

                ICurve wallOpeningOutline = ToPolyline(openingPts);

                Opening bhomOpening = Structure.Create.Opening(wallOpeningOutline);
                bhomWallOpenings.Add(bhomOpening);
            }

            //  Create wall
            Panel bhomPanel = Structure.Create.Panel(outline,bhomWallOpenings);

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Wall");


            //Extract properties
            List<string> CustomProps = new List<string>();
            EMATERIALTYPES material = ramWall.eMaterial;

            //Get wall section property
            ConstantThickness wall2DProp = new ConstantThickness();
            string wallLabel = "";
            double wallThickness = ramWall.dThickness;
            IMaterialFragment Material = null;

            if (ramWall.eMaterial == EMATERIALTYPES.EWallPropConcreteMat)
            {
                wallLabel = "Concrete " + wallThickness.ToString();
                Material = Engine.Structure.Create.Concrete("Concrete");
            }
            else
            {
                wallLabel = "Other " + wallThickness.ToString();
                Material = Engine.Structure.Create.Concrete("Other");
            }

            wall2DProp.Name = wallLabel;
            wall2DProp.Thickness = wallThickness;
            wall2DProp.PanelType = PanelType.Wall;
            wall2DProp.Material = Material;

            bhomPanel.Property = wall2DProp;
            bhomPanel.Tags = tag;
            bhomPanel.Name = ramWall.lLabel.ToString();

            // Add custom data
            bhomPanel.CustomData["lUID"] = ramWall.lUID;
            bhomPanel.Tags.Add("Wall");

            return bhomPanel;
        }

        /***************************************************/

        public static Node ToBHoMObject(this INode INode)
        {

            // Get the location of the node
            SCoordinate Location = new SCoordinate();
            Location = INode.sLocation;
            
            Node Node = Structure.Create.Node(new oM.Geometry.Point() { X = Location.dXLoc, Y = Location.dYLoc, Z = Location.dZLoc });

            IMemberForces IMemberForces = INode.GetReactions();

            // Collect all member forces at node, tracked by index; should these be combined?
            for (int i = 0; i < IMemberForces.GetCount(); i++)
            {
                IMemberForce IMemberForce = IMemberForces.GetAt(i);
                double axial = IMemberForce.dAxial;
                double loc = IMemberForce.dLocation;
                double momMaj = IMemberForce.dMomentMajor;
                double momMin = IMemberForce.dMomentMinor;
                double shearMaj = IMemberForce.dShearMajor;
                double shearMin = IMemberForce.dShearMinor;
                double torsion = IMemberForce.dTorsion;
                string loadcaseID = IMemberForce.lLoadCaseID.ToString();

                //Set by member number--is there a way to do this in nested lists instead?
                Node.CustomData.Add("Axial" + i.ToString(), axial);
                Node.CustomData.Add("Location" + i.ToString(), loc);
                Node.CustomData.Add("Moment Major" + i.ToString(), momMaj);
                Node.CustomData.Add("Moment Minor" + i.ToString(), momMin);
                Node.CustomData.Add("Shear Major" + i.ToString(), shearMaj);
                Node.CustomData.Add("Shear Minor" + i.ToString(), shearMin);
                Node.CustomData.Add("Torsion" + i.ToString(), torsion);
                Node.CustomData.Add("Loadcase_ID" + i.ToString(), loadcaseID);
            }

            return Node;
        }

        /***************************************************/

        public static Loadcase ToBHoMObject(this ILoadCase ILoadCase)
        {

            Loadcase Loadcase = new Loadcase();
            Loadcase.Name = ILoadCase.strSymbol;
            Loadcase.Number = ILoadCase.lUID;
            ELoadCaseType LoadType = ILoadCase.eLoadType;
            switch (LoadType)
            {
                case ELoadCaseType.NotionalDeadLCa:
                case ELoadCaseType.DeadLCa:
                case ELoadCaseType.ConstructionDeadLCa:
                case ELoadCaseType.MassDeadLCa:
                    Loadcase.Nature = LoadNature.Dead;
                    break;
                case ELoadCaseType.ConstructionLiveLCa:
                case ELoadCaseType.LiveLCa:
                case ELoadCaseType.LiveReducibleLCa:
                case ELoadCaseType.LiveRoofLCa:
                case ELoadCaseType.LiveStorageLCa:
                case ELoadCaseType.LiveUnReducibleLCa:
                case ELoadCaseType.NotionalLiveLCa:
                case ELoadCaseType.PartitionLCa:
                    Loadcase.Nature = LoadNature.Live;
                    break;
                case ELoadCaseType.SnowLCa:
                    Loadcase.Nature = LoadNature.Snow;
                    break;
                case ELoadCaseType.SeismicLCa:
                    Loadcase.Nature = LoadNature.Seismic;
                    break;
                case ELoadCaseType.WindLCa:
                    Loadcase.Nature = LoadNature.Wind;
                    break;
                default:
                    Loadcase.Nature = LoadNature.Other;
                    break;
            }

            return Loadcase;
        }

        /***************************************************/

        public static LoadCombination ToBHoMObject(IModel IModel,ILoadCombination ILoadCombination)
        {

            LoadCombination LoadCombination = new LoadCombination();
            LoadCombination.Name = ILoadCombination.strDisplayString;
            LoadCombination.Number = ILoadCombination.lLabelNo;

            ILoadCombinationTerms ILoadCombinationTerms = ILoadCombination.GetLoadCombinationTerms();

            for (int i = 0; i < ILoadCombinationTerms.GetCount(); i++)
            {
                //Get LoadCombination Cases and Factors
                ILoadCombinationTerm ILoadCombinationTerm = ILoadCombinationTerms.GetAt(i);
                int caseID = ILoadCombinationTerm.lLoadCaseID;
                ILoadCases ILoadCases = IModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);
                ILoadCase ILoadCase = ILoadCases.Get(caseID);

                //Convert Loadcase from RAM to BHoM
                Loadcase bhomLoadcase = ToBHoMObject(ILoadCase);
                //Add dict for load factor and loadcase
                LoadCombination.LoadCases.Add(new Tuple<double, ICase>(ILoadCombinationTerm.dFactor, bhomLoadcase));
                
            }

            return LoadCombination;
        }

        /***************************************************/

        public static NodeReaction ToBHoMObject(this IPointLoad ramPointLoad, ILoadCase ramLoadCase)
        {
            SCoordinate ramPoint;
            ramPointLoad.GetCoordinate(out ramPoint);

            string ramPointID = ramPoint.dXLoc.ToString() + ", " + ramPoint.dYLoc.ToString() + ", " + ramPoint.dZLoc.ToString() + ", "; // no object id option for RAM nodes, id by coordinates instead
            NodeReaction bhomNodeReaction = new NodeReaction
            {
                ResultCase = ramLoadCase.strLoadCaseGroupLabel + ramLoadCase.strTypeLabel,
                ObjectId = ramPointID,
                FX = ramPointLoad.dFx,
                FY = ramPointLoad.dFy,
                FZ = ramPointLoad.dFz,
                MX = ramPointLoad.dMxx,
                MY = ramPointLoad.dMyy,
                MZ = ramPointLoad.dMzz
            };

            return bhomNodeReaction;
        }

        /***************************************************/

        public static NodeReaction ToBHoMObject(this IMemberForce ramForce)
        {
            NodeReaction bhomNodeReaction = new NodeReaction
            {
                FX = ramForce.dAxial,
                FY = ramForce.dShearMinor,
                FZ = ramForce.dShearMajor,
                MX = ramForce.dTorsion,
                MY = ramForce.dMomentMajor,
                MZ = ramForce.dMomentMinor
            };

            return bhomNodeReaction;
        }

        /***************************************************/

        public static Grid ToBHoMObject(this IModelGrid IModelGrid, IGridSystem IGridSystem, int counter)
        {
            Grid myGrid = new Grid();
            // Get the parameters of Gridsystem 
            string gridSystemLabel = IGridSystem.strLabel;// Set the name of the GridSystem from RAM
            int gridSystemID = IGridSystem.lUID;    //Set the lUID from RAM
            string gridSystemType = IGridSystem.eOrientationType.ToString();// Set the orientation type
            double gridXoffset = IGridSystem.dXOffset;   // Set the offset of the GridSystem from 0 along the X axis
            double gridYoffset = IGridSystem.dYOffset; // Set the offset of the GridSystem from 0 along the Y axis
            double gridSystemRotation = IGridSystem.dRotation; // Set the rotation angle of the GridSystem
            double gridRotAngle = 0;

            // Add the properties of the GridSystem as CustomData 
            myGrid.CustomData.Add("lUID", gridSystemID);
            myGrid.CustomData.Add("RAMLabel", gridSystemLabel);
            myGrid.CustomData.Add("RamGridType", gridSystemType);
            myGrid.CustomData.Add("xOffset", gridXoffset);
            myGrid.CustomData.Add("yOffset", gridYoffset);
            myGrid.CustomData.Add("RamGridRotation", gridSystemRotation);

            //Get info for each grid line
            int gridLinelUID = IModelGrid.lUID; //unique ID od of the grid line object
            string gridLineLabel = IModelGrid.strLabel; // label of the gridline
            double gridLineCoord_Angle = IModelGrid.dCoordinate_Angle; // the grid coordinate or angle
            string gridLineAxis = IModelGrid.eAxis.ToString(); // grid line axis , X/Radial Y/Circular 

            double dMaxLimit = IModelGrid.dMaxLimitValue; // maximum limit specified by the user to which gridline will be drawn from origin
            double dMinLimit = IModelGrid.dMinLimitValue; // minimum limit specified by the user to which gridline will be drawn from origin
            double GridLength = 3000; //default grid length value

            //Set max and min limit values based on if they are used or if -1 is returned
            if (dMaxLimit != 0)
            {
                GridLength = 0;
            }

            //Set Grid start offset from system origin
            double spacing = 0;
            spacing = IModelGrid.dCoordinate_Angle;

            Point gridCoordPoint1 = new Point();
            Point gridCoordPoint2 = new Point();

            if (gridSystemType == "eGridOrthogonal")   // code to place grids in orthogonal X and Y
            {
                //check the orientation to place grides accordingly
                if (gridLineAxis == "eGridXorRadialAxis")
                {

                    // position of first point
                    gridCoordPoint1.X = gridXoffset + gridLineCoord_Angle; // at the origin point we add the spacing of the grid 
                    gridCoordPoint1.Y = gridYoffset + dMinLimit;
                    gridCoordPoint1.Z = 0;
                    // position of second point
                    gridCoordPoint2.X = gridXoffset + gridLineCoord_Angle;
                    gridCoordPoint2.Y = gridYoffset + GridLength + dMaxLimit;// add the max limit to the origin point to get full length of gridline
                    gridCoordPoint2.Z = 0;

                }
                else if (gridLineAxis == "eGridYorCircularAxis")
                {
                    // position of first point
                    gridCoordPoint1.X = gridXoffset + dMinLimit; // at the origin point we add the coordinate of the grid 
                    gridCoordPoint1.Y = gridYoffset + gridLineCoord_Angle;
                    gridCoordPoint1.Z = 0;
                    // position of second point
                    gridCoordPoint2.X = gridXoffset + GridLength + dMaxLimit; // add the max limit to the origin point to get full length of gridline
                    gridCoordPoint2.Y = gridYoffset + gridLineCoord_Angle;
                    gridCoordPoint2.Z = 0;

                }
                // initialize a new line to create the gridline
                Line gridLine = new Line();
                gridLine.Start = gridCoordPoint1;
                gridLine.End = gridCoordPoint2;

                //Create a new grid object from the drawn line and return it
                myGrid = new Grid { Curve = gridLine, Name = gridLineLabel };
            }
            else if (gridSystemType == "eGridRadial")  //code to place grids radially
            {
                gridRotAngle = (Math.PI / 180) * (gridLineCoord_Angle + gridSystemRotation);
                if (gridLineAxis == "eGridXorRadialAxis")
                {
                    // position of first point
                    gridCoordPoint1.X = gridXoffset; // at the origin point we add the coordinate of the grid 
                    gridCoordPoint1.Y = gridYoffset;
                    gridCoordPoint1.Z = 0;
                    // position of second point
                    gridCoordPoint2.X = gridXoffset + Math.Cos(gridRotAngle) *(GridLength + dMaxLimit); // add the max limit to the origin point to get full length of gridline
                    gridCoordPoint2.Y = gridYoffset + Math.Sin(gridRotAngle) * (GridLength + dMaxLimit);
                    gridCoordPoint2.Z = 0;
                    
                    // initialize a new line to create the gridline
                    Line gridLine = new Line();
                    gridLine.Start = gridCoordPoint1;
                    gridLine.End = gridCoordPoint2;

                    //Create a new grid object from the drawn line and return it
                    myGrid = new Grid { Curve = gridLine, Name = gridLineLabel };
                }
                else if (gridLineAxis == "eGridYorCircularAxis")
                {
                    // position of first point
                    gridCoordPoint1.X = gridXoffset; // at the origin point we add the coordinate of the grid 
                    gridCoordPoint1.Y = gridYoffset;
                    gridCoordPoint1.Z = 0;

                    // initialize a new line to create the gridline
                    Circle gridLine = new Circle();
                    Vector cirNormal = new Vector { X = 0, Y = 0, Z = 1 };
                    gridLine.Centre = gridCoordPoint1;
                    gridLine.Normal = cirNormal;
                    gridLine.Radius = gridLineCoord_Angle;

                    //Create a new grid object from the drawn line and return it
                    myGrid = new Grid { Curve = gridLine, Name = gridLineLabel };
                }
            }
            
            /// end of Grid toBhomObject method
            return myGrid;

        }

        /***************************************************/

        public static Level ToBHoMObject(this IStory story)
        {
            Level bhomLevel = new Level
            {
                Elevation = story.dElevation,
                Name = story.strLabel
            };

            return bhomLevel;
        }

        /***************************************************/

    }
}
