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
using System.Collections.Generic;
using System.Linq;
using BH.oM.Geometry;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Spatial.SettingOut;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Loads;
using BH.oM.Structure.Constraints;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.Results;
using BH.oM.Adapters.RAM;
using BH.Engine.Units;
using RAMDATAACCESSLib;
using BH.Engine.Adapter;
using BH.Engine.Adapters.RAM;
using BH.oM.Adapter;
using BH.Engine.Geometry;
using BH.Engine.Structure;
using BH.Engine.Base;

namespace BH.Adapter.RAM
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<double> ToLevelElevations(this IModel ramModel)
        {
            List<double> RAMLevelHeights = new List<double>();

            //Get existing levels
            List<string> FloorTypeNames = new List<string>();
            List<string> StoryNames = new List<string>();
            double StoryElevation;
            IFloorTypes IFloorTypes = ramModel.GetFloorTypes();
            IStories IStories = ramModel.GetStories();

            double storyCount = IStories.GetCount();

            for (int i = 0; i < storyCount; i++)
            {
                StoryElevation = IStories.GetAt(i).dElevation.FromInch();
                RAMLevelHeights.Add(StoryElevation);
            }
            return RAMLevelHeights;
        }

        /***************************************************/
        
        public static Polyline ToPolyline(this IPoints ramPoints, double zShift = 0)
        {
            List<Point> controlPts = new List<Point>();
            SCoordinate SCoordPt = new SCoordinate();

            for (int i = 0; i < ramPoints.GetCount(); i++)
            {
                //Get Polyline Pts
                IPoint IPoint = ramPoints.GetAt(i);
                IPoint.GetCoordinate(ref SCoordPt);
                Point controlPt = SCoordPt.PointFromRAM(zShift);
                controlPts.Add(controlPt);
            }

            Polyline polyline = new Polyline();
            polyline.ControlPoints = controlPts;
            return polyline;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IBeam ramBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();
            IMaterialFragment Material = null;

            if (ramBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
            }
            else if (ramBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
            }
            else
            {
                Material = Engine.Structure.Create.Steel("Other");
            }
            sectionProperty.Material = Material;
            sectionProperty.Name = ramBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IColumn ramBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (ramBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (ramBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = ramBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IVerticalBrace ramBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (ramBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (ramBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = ramBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this IHorizBrace ramBar)
        {
            //Create BHoM SectionProperty
            ISectionProperty sectionProperty = new ExplicitSection();

            IMaterialFragment Material = null;

            if (ramBar.eMaterial == EMATERIALTYPES.EConcreteMat)
            {
                Material = Engine.Structure.Create.Concrete("Concrete");
                //sectionProperty = Create.ConcreteRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, Material, sectionName);
            }
            else if (ramBar.eMaterial == EMATERIALTYPES.ESteelMat)
            {
                Material = Engine.Structure.Create.Steel("Steel");
                //sectionProperty = Create.SteelRectangleSection(IBeam.dWebDepth, IBeam.dFlangeWidthTop, 0,Material,sectionName);
            }

            sectionProperty.Material = Material;
            sectionProperty.Name = ramBar.strSectionLabel;

            return sectionProperty;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IColumn ramColumn)
        {

            // Get the column name
            string section = ramColumn.strSectionLabel;

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            ramColumn.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = new Node { Position = startPt.PointFromRAM() };
            Node endNode = new Node { Position = endPt.PointFromRAM() };
            
            //Assign section property per bar
            string sectionName = ramColumn.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(ramColumn);

            // Create bars with section properties
            Bar bhomBar = new Bar { Start = startNode, End = endNode, SectionProperty = sectionProperty, Name = sectionName };

            // Translate RAM Releases to BHoM Releases (in progress; logic not yet complete since it is difficult map Maj/Min axes to global XYZ axes for every member)
            // May be better to just do in custom data, although if we can do this mapping it may be useful
            bhomBar.Release = new BarRelease();
            bhomBar.Release.StartRelease = new Constraint6DOF();
            bhomBar.Release.EndRelease = new Constraint6DOF();
            bhomBar.Release.StartRelease.RotationX = new DOFType();
            bhomBar.Release.EndRelease.RotationX = new DOFType();
            bhomBar.Release.StartRelease.RotationY = new DOFType();
            bhomBar.Release.EndRelease.RotationY = new DOFType();

            if (ramColumn.bMajAxisBendFixedTop == 1) { bhomBar.Release.StartRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationX = DOFType.Free; }
            if (ramColumn.bMajAxisBendFixedBot == 1) { bhomBar.Release.EndRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationX = DOFType.Free; }
            if (ramColumn.bMinAxisBendFixedTop == 1) { bhomBar.Release.StartRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationY = DOFType.Free; }
            if (ramColumn.bMinAxisBendFixedBot == 1) { bhomBar.Release.EndRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationY = DOFType.Free; }


            bhomBar.OrientationAngle = 0;

            // Add RAM Unique ID, custom Data
            RAMId RAMId = new RAMId();
            RAMId.Id = ramColumn.lUID;
            bhomBar.SetAdapterId(RAMId);

            //Add Frame Data fragment
            RAMFrameData ramFrameData = new RAMFrameData();
            ramFrameData.FrameNumber = ramColumn.lLabel;
            ramFrameData.FrameType = ramColumn.eFramingType.ToString();
            ramFrameData.Material = ramColumn.eMaterial.ToString();
            ramFrameData.IsHangingColumn = (ramColumn.bHanger == 1);
            bhomBar.Fragments.Add(ramFrameData);

            bhomBar.Tags.Add("Column");

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IBeam ramBeam, ILayoutBeam ramLayoutBeam, double dElevation)
        {

            // Get coordinates from IBeam
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            ramBeam.GetCoordinates(EBeamCoordLoc.eBeamEnds, ref startPt, ref endPt);
            Node startNode = new Node { Position = startPt.PointFromRAM() };
            Node endNode = new Node { Position = endPt.PointFromRAM() };

            //Assign section property per bar
            string sectionName = ramBeam.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(ramBeam);

            // Create bars with section properties
            Bar bhomBar = new Bar { Start = startNode, End = endNode, SectionProperty = sectionProperty, Name = sectionName };

            // Set Properties
            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            RAMId RAMId = new RAMId();
            RAMId.Id = ramBeam.lUID;
            bhomBar.SetAdapterId(RAMId);

            RAMFrameData ramFrameData = new RAMFrameData();
            ramFrameData.FrameNumber = ramBeam.lLabel;
            ramFrameData.StartCantilever = ramBeam.dStartCantilever.FromInch();
            ramFrameData.EndCantilever = ramBeam.dEndCantilever.FromInch();
            ramFrameData.IsStubCantilever = (ramLayoutBeam.IsStubCantilever() == 1);
            ramFrameData.FrameType = ramBeam.eFramingType.ToString();
            ramFrameData.Material = ramBeam.eMaterial.ToString();         

            bhomBar.Tags.Add("Beam");


            // Get Steel beam results
            ISteelBeamDesignResult Result = ramBeam.GetSteelDesignResult();
            DAArray ppalNumStuds = Result.GetNumStudsInSegments();

            int numStudSegments = new int();
            ppalNumStuds.GetSize(ref numStudSegments);
            double camber = ramBeam.dCamber;
            int studCount = 0;

            IAnalyticalResult AnalyticalResult = ramBeam.GetAnalyticalResult();
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
                ramFrameData.Studs = studCount;
            }

            // Add camber to Custom Data
            if (camber > Double.MinValue)
            {
                ramFrameData.Camber = camber.FromInch();
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

            if (ramBeam.bMajAxisBendFixedStart == 1) { bhomBar.Release.StartRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationX = DOFType.Free; }
            if (ramBeam.bMajAxisBendFixedEnd == 1) { bhomBar.Release.EndRelease.RotationX = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationX = DOFType.Free; }
            if (ramBeam.bMinAxisBendFixedStart == 1) { bhomBar.Release.StartRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.StartRelease.RotationY = DOFType.Free; }
            if (ramBeam.bMinAxisBendFixedEnd == 1) { bhomBar.Release.EndRelease.RotationY = DOFType.Fixed; } else { bhomBar.Release.EndRelease.RotationY = DOFType.Free; }

            double DCI = Result.dDesignCapacityInteraction;
            double CDI = Result.dCriticalDeflectionInteraction;

            // Add DCI and CDI data
            ramFrameData.DesignCapacityInteraction = DCI;
            ramFrameData.CriticalDeflectionInteraction = CDI;

            bhomBar.Fragments.Add(ramFrameData);

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(this IVerticalBrace ramVerticalBrace)
        {

            // Get the column name
            string sectionName = ramVerticalBrace.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(ramVerticalBrace);

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            ramVerticalBrace.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = new Node { Position = startPt.PointFromRAM() };
            Node endNode = new Node { Position = endPt.PointFromRAM() };


            Bar bhomBar = new Bar { Start = startNode, End = endNode, SectionProperty = sectionProperty, Name = sectionName };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            RAMId RAMId = new RAMId();
            RAMId.Id = ramVerticalBrace.lUID;
            bhomBar.SetAdapterId(RAMId);

            //Add Frame Data fragment
            RAMFrameData ramFrameData = new RAMFrameData();
            ramFrameData.FrameNumber = ramVerticalBrace.lLabel;
            ramFrameData.FrameType = ramVerticalBrace.eSeismicFrameType.ToString();
            ramFrameData.Material = ramVerticalBrace.eMaterial.ToString();
            bhomBar.Fragments.Add(ramFrameData);

            bhomBar.Tags.Add("VerticalBrace");

            return bhomBar;
        }

        /***************************************************/

        public static Bar ToBHoMObject(IHorizBrace ramHorizBrace, ILayoutHorizBrace ramLayoutHorizBrace, double dElevation)
        {

            string sectionName = ramHorizBrace.strSectionLabel;

            ISectionProperty sectionProperty = ToBHoMSection(ramHorizBrace);

            // Get the start and end pts of every brace
            double StartSupportX = new double();
            double StartSupportY = new double();
            double StartSupportZOffset = new double();
            double EndSupportX = new double();
            double EndSupportY = new double();
            double EndSupportZOffset = new double();
            double StoryZ = dElevation;


            // Get coordinates from ILayout Brace
            ramLayoutHorizBrace.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
            Node startNode = new Node { Position = new oM.Geometry.Point() { X = StartSupportX.FromInch(), Y = StartSupportY.FromInch(), Z = StoryZ.FromInch() + StartSupportZOffset.FromInch() } };
            Node endNode = new Node { Position = new oM.Geometry.Point() { X = EndSupportX.FromInch(), Y = EndSupportY.FromInch(), Z = StoryZ.FromInch() + EndSupportZOffset.FromInch() } };

            Bar bhomBar = new Bar { Start = startNode, End = endNode, SectionProperty = sectionProperty, Name = sectionName };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            RAMId RAMId = new RAMId();
            RAMId.Id = ramLayoutHorizBrace.lUID;
            bhomBar.SetAdapterId(RAMId);

            //Add Frame Data fragment
            RAMFrameData ramFrameData = new RAMFrameData();
            ramFrameData.FrameNumber = ramLayoutHorizBrace.lLabel;
            ramFrameData.Material = ramLayoutHorizBrace.eMaterial.ToString();
            bhomBar.Fragments.Add(ramFrameData);

            bhomBar.Tags.Add("HorizontalBrace");

            return bhomBar;
        }

        /***************************************************/

        public static Panel ToBHoMObject(IDeck ramDeck, IModel ramModel, int ramStoryUID)
        {

            //Get panel props
            EDeckType type = ramDeck.eDeckPropType;

            //Find polylines of deck in RAM Model

            //get count of deck polygons
            double deckPolyCount = ramDeck.GetNumFinalPolygons(ramStoryUID);

            if (deckPolyCount == 0)
            {
                Engine.Base.Compute.RecordWarning($"Floor with RAM lUID {ramDeck.lUID} contains no edges.");
                return null;
            }
            //Initial only gets first outline poly for exterior edge, rest for openings
            IPoints pplPoints = ramDeck.GetFinalPolygon(ramStoryUID, 0);

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
                IPoints openingPts = ramDeck.GetFinalPolygon(ramStoryUID, i);

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
            Panel bhomPanel = Engine.Structure.Create.Panel(outline);
            //Create openings per openings polylines
            int numOpenings = openingPLs.Count();

            //Create openings
            for (int i = 0; i < numOpenings; i++)
            {
                Opening bhomOpening = Engine.Structure.Create.Opening(openingPLs[i]);
                bhomPanel.Openings.Add(bhomOpening);
            }

            //Create panel and add attributes;

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Floor");

            bhomPanel.Tags = tag;
            bhomPanel.Name = type.ToString();

            RAMId RAMId = new RAMId();
            RAMId.Id = ramDeck.lUID;
            bhomPanel.SetAdapterId(RAMId);

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
            corners.Add(TopstartPt.PointFromRAM());
            corners.Add(TopendPt.PointFromRAM());
            corners.Add(BottomendPt.PointFromRAM());
            corners.Add(BottomstartPt.PointFromRAM());
            corners.Add(TopstartPt.PointFromRAM());

            // Create outline from corner points
            Polyline outline = new Polyline();
            outline.ControlPoints = corners;

            //Create opening outlines
            List<ICurve> wallOpeningPLs = new List<ICurve>();
            List<Opening> bhomWallOpenings = new List<Opening>();

            // Create openings
            IFinalWallOpenings IFinalWallOpenings = ramWall.GetFinalOpenings();
            IRawWallOpenings rawOpenings = ramWall.GetRawOpenings();
            if (rawOpenings.GetCount() > 0)
            {
                IRawWallOpening check = rawOpenings.GetAt(0);
            }

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

                Opening bhomOpening = Engine.Structure.Create.Opening(wallOpeningOutline);
                bhomWallOpenings.Add(bhomOpening);
            }

            //  Create wall
            Panel bhomPanel = Engine.Structure.Create.Panel(outline);
            bhomPanel.Openings.AddRange(bhomWallOpenings);

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Wall");

            //Get wall section property
            ConstantThickness wall2DProp = new ConstantThickness();
            string wallLabel = "";
            double wallThickness = ramWall.dThickness.FromInch();
            IMaterialFragment Material = null;

            if (ramWall.eMaterial == EMATERIALTYPES.EWallPropConcreteMat)
            {
                wallLabel = "Concrete " + ramWall.dThickness.ToString() + " in";
                Material = Engine.Structure.Create.Concrete("Concrete");
            }
            else
            {
                wallLabel = "Other " + ramWall.dThickness.ToString() + " in";
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
            RAMId RAMId = new RAMId();
            RAMId.Id = ramWall.lUID;
            bhomPanel.SetAdapterId(RAMId);
            bhomPanel.Tags.Add("Wall");

            return bhomPanel;

        }

        /***************************************************/

        public static Node ToBHoMObject(this INode ramNode)
        {

            // Get the location of the node
            SCoordinate location = new SCoordinate();
            location = ramNode.sLocation;
            
            Node node = new Node { Position = location.PointFromRAM() };

            IMemberForces IMemberForces = ramNode.GetReactions();

            // Collect all member forces at node, tracked by index; should these be combined?
            for (int i = 0; i < IMemberForces.GetCount(); i++)
            {
                RAMNodeForceData ramNodeForceData = new RAMNodeForceData();

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
                ramNodeForceData.Axial = axial;
                ramNodeForceData.Location = loc;
                ramNodeForceData.MomMaj = momMaj;
                ramNodeForceData.MomMin = momMin;
                ramNodeForceData.ShearMaj = shearMaj;
                ramNodeForceData.ShearMin = shearMin;
                ramNodeForceData.Torsion = torsion;
                ramNodeForceData.LoadcaseID = loadcaseID;

                node.Fragments.Add(ramNodeForceData);
            }

            return node;
        }

        /***************************************************/

        public static Loadcase ToBHoMObject(this ILoadCase ramLoadCase)
        {

            Loadcase Loadcase = new Loadcase();
            Loadcase.Name = ramLoadCase.strSymbol;
            Loadcase.Number = ramLoadCase.lUID;
            ELoadCaseType LoadType = ramLoadCase.eLoadType;
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

        public static LoadCombination ToBHoMObject(IModel ramModel,ILoadCombination ramLoadCombination)
        {

            LoadCombination loadCombination = new LoadCombination();
            loadCombination.Name = ramLoadCombination.strDisplayString;
            loadCombination.Number = ramLoadCombination.lLabelNo;

            ILoadCombinationTerms iLoadCombinationTerms = ramLoadCombination.GetLoadCombinationTerms();

            for (int i = 0; i < iLoadCombinationTerms.GetCount(); i++)
            {
                //Get LoadCombination Cases and Factors
                ILoadCombinationTerm iLoadCombinationTerm = iLoadCombinationTerms.GetAt(i);
                int caseID = iLoadCombinationTerm.lLoadCaseID;
                ILoadCases iLoadCases = ramModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);
                ILoadCase iLoadCase = iLoadCases.Get(caseID);

                //Convert Loadcase from RAM to BHoM
                Loadcase bhomLoadcase = ToBHoMObject(iLoadCase);
                //Add dict for load factor and loadcase
                loadCombination.LoadCases.Add(new Tuple<double, ICase>(iLoadCombinationTerm.dFactor, bhomLoadcase));
                
            }

            return loadCombination;
        }

        /***************************************************/

        public static NodeReaction ToBHoMObject(this IPointLoad ramPointLoad, ILoadCase ramLoadCase)
        {
            SCoordinate ramPoint;
            ramPointLoad.GetCoordinate(out ramPoint);
            Point bhomPoint = ramPoint.PointFromRAM();
            string ramPointID = bhomPoint.X.ToString() + ", " + bhomPoint.Y.ToString() + ", " + bhomPoint.Z.ToString() + ", "; // no object id option for RAM nodes, id by coordinates instead

            //TODO: resolve below identifiers extractable through the API
            int mode = -1;
            double timeStep = 0;

            NodeReaction bhomNodeReaction = new NodeReaction(
                ramPointID,
                ramLoadCase.strLoadCaseGroupLabel + ramLoadCase.strTypeLabel,
                mode,
                timeStep,
                Basis.XY,
                ramPointLoad.dFx,
                ramPointLoad.dFy,
                ramPointLoad.dFz,
                ramPointLoad.dMxx,
                ramPointLoad.dMyy,
                ramPointLoad.dMzz
                );

            return bhomNodeReaction;
        }

        /***************************************************/

        public static NodeReaction ToBHoMObject(this IMemberForce ramForce)
        {
            NodeReaction bhomNodeReaction = new NodeReaction("", "", -1, 0, Basis.XY, ramForce.dAxial, ramForce.dShearMinor, ramForce.dShearMajor, ramForce.dTorsion, ramForce.dMomentMajor, ramForce.dMomentMinor);
            return bhomNodeReaction;
        }

        /***************************************************/

        public static Grid ToBHoMObject(this IModelGrid ramModelGrid, IGridSystem ramGridSystem, int counter)
        {
            Grid myGrid = new Grid();
            // Get the parameters of Gridsystem 
            string gridSystemLabel = ramGridSystem.strLabel;// Set the name of the GridSystem from RAM
            int gridSystemID = ramGridSystem.lUID;    //Set the lUID from RAM
            string gridSystemType = ramGridSystem.eOrientationType.ToString();// Set the orientation type
            double gridXoffset = ramGridSystem.dXOffset.FromInch();   // Set the offset of the GridSystem from 0 along the X axis
            double gridYoffset = ramGridSystem.dYOffset.FromInch(); // Set the offset of the GridSystem from 0 along the Y axis
            double gridSystemRotation = ramGridSystem.dRotation; // Set the rotation angle of the GridSystem
            double gridRotAngle = 0;

            // Add the properties of the GridSystem as a fragment
            RAMId RAMId = new RAMId();
            RAMId.Id = gridSystemID;
            myGrid.SetAdapterId(RAMId);

            RAMGridData ramGridData = new RAMGridData();
            ramGridData.Label = gridSystemLabel;
            ramGridData.Type = gridSystemType;
            ramGridData.XOffset = gridXoffset;
            ramGridData.YOffset = gridYoffset;
            ramGridData.Rotation = gridSystemRotation;

            //Get info for each grid line
            int gridLinelUID = ramModelGrid.lUID; //unique ID od of the grid line object
            string gridLineLabel = ramModelGrid.strLabel; // label of the gridline
            double gridLineCoord_Angle = ramModelGrid.dCoordinate_Angle; // the grid coordinate or angle
            string gridLineAxis = ramModelGrid.eAxis.ToString(); // grid line axis , X/Radial Y/Circular 

            double dMaxLimit = ramModelGrid.dMaxLimitValue.FromInch(); // maximum limit specified by the user to which gridline will be drawn from origin
            double dMinLimit = ramModelGrid.dMinLimitValue.FromInch(); // minimum limit specified by the user to which gridline will be drawn from origin
            double GridLength = 100; //default grid length value

            //Set max and min limit values based on if they are used or if -1 is returned
            if (dMaxLimit != 0)
            {
                GridLength = 0;
            }



            Point gridCoordPoint1 = new Point();
            Point gridCoordPoint2 = new Point();

            if (gridSystemType == "eGridOrthogonal")   // code to place grids in orthogonal X and Y
            {
                //Set Grid start offset from system origin
                double spacing = ramModelGrid.dCoordinate_Angle.FromInch();
                
                //check the orientation to place grids accordingly
                if (gridLineAxis == "eGridXorRadialAxis")
                {

                    // position of first point
                    gridCoordPoint1.X = gridXoffset + spacing; // at the origin point we add the spacing of the grid 
                    gridCoordPoint1.Y = gridYoffset + dMinLimit;
                    gridCoordPoint1.Z = 0;
                    // position of second point
                    gridCoordPoint2.X = gridXoffset + spacing;
                    gridCoordPoint2.Y = gridYoffset + GridLength + dMaxLimit;// add the max limit to the origin point to get full length of gridline
                    gridCoordPoint2.Z = 0;

                }
                else if (gridLineAxis == "eGridYorCircularAxis")
                {
                    // position of first point
                    gridCoordPoint1.X = gridXoffset + dMinLimit; // at the origin point we add the coordinate of the grid 
                    gridCoordPoint1.Y = gridYoffset + spacing;
                    gridCoordPoint1.Z = 0;
                    // position of second point
                    gridCoordPoint2.X = gridXoffset + GridLength + dMaxLimit; // add the max limit to the origin point to get full length of gridline
                    gridCoordPoint2.Y = gridYoffset + spacing;
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
                    gridLine.Radius = gridLineCoord_Angle.FromInch();

                    //Create a new grid object from the drawn line and return it
                    myGrid = new Grid { Curve = gridLine, Name = gridLineLabel };
                }
            }
            
            /// end of Grid toBhomObject method
            return myGrid;

        }

        /***************************************************/

        public static Level ToBHoMObject(this IStory ramStory)
        {
            Level bhomLevel = new Level
            {
                Elevation = ramStory.dElevation.FromInch(),
                Name = ramStory.strLabel
            };

            return bhomLevel;
        }

        /***************************************************/

        public static ContourLoadSet ToBHoMObject(this ISurfaceLoadSet ramSrfLoadSet, IStory ramStory)
        {
            // Get srf load outline
            List<Point> srfLoadContourPts = new List<Point>();
            double elev = ramStory.dElevation;

            IPoints srfPolyPts = ramSrfLoadSet.GetPoints();

            Polyline srfLoadContour = ToPolyline(srfPolyPts, elev);

            ContourLoadSet srfLoad = new ContourLoadSet
            {
                Contour = srfLoadContour
            };

            // Unique RAM ID
            RAMId RAMId = new RAMId();
            RAMId.Id = ramSrfLoadSet.lUID;
            srfLoad.SetAdapterId(RAMId);

            return srfLoad;
        }

        /***************************************************/

        public static UniformLoadSet ToBHoMObject(this ISurfaceLoadPropertySet ramSrfLoadPropSet)
        {
            RAMLiveLoadTypes liveType = RAMLiveLoadTypes.LiveUnReducibleLCa;

            if (ramSrfLoadPropSet.eLiveLoadType != ELoadCaseType.DeadLCa)
            {
                liveType = ramSrfLoadPropSet.eLiveLoadType.ToBHoM();
            }
            else 
            {
                Engine.Base.Compute.RecordWarning($"Live Load type for load set {ramSrfLoadPropSet.strLabel} was set as Dead. This usually means that no live load is applied in RAM; check results.");
            }

            UniformLoadSet uniformLoadSet = Engine.Adapters.RAM.Create.RAMUniformLoadSet(
                ramSrfLoadPropSet.dDeadLoad.FromKilopoundForcePerSquareInch(),
                ramSrfLoadPropSet.dConstDeadLoad.FromKilopoundForcePerSquareInch(),
                ramSrfLoadPropSet.dConstLiveLoad.FromKilopoundForcePerSquareInch(),
                liveType,
                ramSrfLoadPropSet.dPartitionLoad.FromKilopoundForcePerSquareInch(),
                ramSrfLoadPropSet.dConstLiveLoad.FromKilopoundForcePerSquareInch(),
                ramSrfLoadPropSet.dMassDeadLoad.FromKilopoundForcePerSquareInch(),
                ramSrfLoadPropSet.strLabel
                );

            // Unique RAM ID
            RAMId RAMId = new RAMId();
            RAMId.Id = ramSrfLoadPropSet.lUID;
            uniformLoadSet.SetAdapterId(RAMId);

            return uniformLoadSet;
        }

        /***************************************************/

        public static RAMLiveLoadTypes ToBHoM(this ELoadCaseType caseType)
        {
            switch (caseType)
            {
                case ELoadCaseType.LiveLCa:
                    return RAMLiveLoadTypes.LiveReducibleLCa;
                case ELoadCaseType.LiveReducibleLCa:
                    return RAMLiveLoadTypes.LiveReducibleLCa;
                case ELoadCaseType.LiveStorageLCa:
                    return RAMLiveLoadTypes.LiveStorageLCa;
                case ELoadCaseType.LiveUnReducibleLCa:
                    return RAMLiveLoadTypes.LiveUnReducibleLCa;
                case ELoadCaseType.LiveRoofLCa:
                    return RAMLiveLoadTypes.LiveRoofLCa;
                default:
                    Engine.Base.Compute.RecordWarning($"Could not convert ELoadCaseType {caseType} to Live Load Type. It might be a non-live load type, but I'm converting it as non-reducible to be safe.");
                    return RAMLiveLoadTypes.LiveUnReducibleLCa;
            }
        }

        /***************************************************/

        public static oM.Geometry.Point PointFromRAM(this SCoordinate sc, double zOffset = 0)
        {
            return BH.Engine.Geometry.Create.Point(sc.dXLoc.FromInch(), sc.dYLoc.FromInch(), sc.dZLoc.FromInch() + zOffset.FromInch());
        }
    }
}






