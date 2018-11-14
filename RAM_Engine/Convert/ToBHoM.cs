using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.oM.Structure.Loads;
using BH.Engine.Structure;
using BH.oM.Structure.Properties;
using RAMDATAACCESSLib;
using BH.oM.Architecture.Elements;

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

        public static List<double> ToLevelElevations(IModel IModel)
        {
            List<double> RAMLevelHeights = new List<double>();

            //Get existing levels
            List<string> FloorTypeNames = new List<string>();
            List<string> StoryNames = new List<string>();
            string FloorTypeName;
            double StoryElevation;
            int FloorID;
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
            
        public static Polyline ToPolyline(IPoints IPoints)
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

        public static Bar ToBHoMObject(IColumn IColumn)
        {

            // Get the column name
            string section = IColumn.strSectionLabel;

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IColumn.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
            endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };


            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IColumn.lUID;
            bhomBar.CustomData["FrameNumber"] = IColumn.lLabel;
            bhomBar.Tags.Add("Column");

            return bhomBar;
        }

        public static Bar ToBHoMObject(IBeam IBeam, ILayoutBeam ILayoutBeam, double dElevation)
        {

            string section = IBeam.strSectionLabel;

            // Get Steel beam results **STILL IN PROGRESS
            ISteelBeamDesignResult Result = IBeam.GetSteelDesignResult();
            DAArray ppalNumStuds = Result.GetNumStudsInSegments();

            int numStudSegments = new int();
            ppalNumStuds.GetSize(ref numStudSegments);
            double Camber = IBeam.dCamber;
            double DCI = Result.dDesignCapacityInteraction;
            double CDI = Result.dCriticalDeflectionInteraction;
            int studCount = 0;

            IAnalyticalResult AnalyticalResult = IBeam.GetAnalyticalResult();
            COMBO_MATERIAL_TYPE Steel_Grav = COMBO_MATERIAL_TYPE.GRAV_STEEL;
            IMemberForces IMemberForces = AnalyticalResult.GetMaximumComboReactions(Steel_Grav);

            // Get coordinates from IBeam
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IBeam.GetCoordinates(EBeamCoordLoc.eBeamEnds, ref startPt, ref endPt);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
            endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };

            //Assign section property per bar

            //get RAM bar section props
            //call RAM to bhom section convert method
            //assign bhom section to bars

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IBeam.lUID;
            bhomBar.CustomData["FrameNumber"] = IBeam.lLabel;
            bhomBar.Tags.Add("Beam");

            //Add studs to custom Data by total stud count only
            for (int i = 0; i < numStudSegments; i++)
            {
                var segStudCount = new object();
                ppalNumStuds.GetAt(i, ref segStudCount);
                string segStudStr = segStudCount.ToString();
                int segStudNum = System.Convert.ToInt16(segStudStr);
                studCount += segStudNum;
                bhomBar.CustomData["Studs"] = studCount;
            }

            // Add camber to Custom Data
            if (Camber > Double.MinValue)
            {
                bhomBar.CustomData["Camber"] = Camber;
            }

            //Commented out for v14 api testing
            //bhomBar.CustomData["Design Capacity Interaction"] = DCI;
            //bhomBar.CustomData["Critical Deflection Interaction"] = CDI;

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

        public static Bar ToBHoMObject(IVerticalBrace IVerticalBrace)
        {

            // Get the column name
            string section = IVerticalBrace.strSectionLabel;

            // Get the start and end pts of every column
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IVerticalBrace.GetEndCoordinates(ref startPt, ref endPt);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
            endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };


            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IVerticalBrace.lUID;
            bhomBar.CustomData["FrameNumber"] = IVerticalBrace.lLabel;
            bhomBar.Tags.Add("VerticalBrace");

            return bhomBar;
        }

        public static Bar ToBHoMObject(IHorizBrace IHorizBrace, ILayoutHorizBrace ILayoutHorizBrace, double dElevation)
        {

            string section = IHorizBrace.strSectionLabel;

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
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset };
            endNode.Position = new BH.oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset };

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            // Unique RAM ID
            bhomBar.CustomData["lUID"] = IHorizBrace.lUID;
            bhomBar.CustomData["FrameNumber"] = IHorizBrace.lLabel;
            bhomBar.Tags.Add("HorizontalBrace");

            return bhomBar;
        }

        public static PanelPlanar ToBHoMObject(IWallPanel IWallPanel)
        {

            //Find corner points of IWallPanel in RAM model
            SCoordinate startPt = IWallPanel.sCoordinateStart;
            SCoordinate endPt = IWallPanel.sCoordinateEnd;

            IWalls IWalls = IWallPanel.GetWalls();
            double maxZ = 0;
            double minZ = 10000000;

            //Go through all walls that are part of the panel; find extreme top and bottom
            for (int i = 0; i < IWalls.GetCount(); i++)
            {
                IWall IWall = IWalls.GetAt(i);

                SCoordinate TopstartPt = new SCoordinate();
                SCoordinate TopendPt = new SCoordinate();
                SCoordinate BottomstartPt = new SCoordinate();
                SCoordinate BottomendPt = new SCoordinate();

                IWall.GetEndCoordinates(ref TopstartPt, ref TopendPt, ref BottomstartPt, ref BottomendPt);

                if (TopendPt.dZLoc > maxZ) { maxZ = TopendPt.dZLoc; }
                if (BottomendPt.dZLoc < minZ) { minZ = BottomendPt.dZLoc; }
            }

            // Create list of points
            List<Point> corners = new List<Point>();
            corners.Add(new Point { X = startPt.dXLoc, Y = startPt.dYLoc, Z = minZ });
            corners.Add(new Point { X = endPt.dXLoc, Y = endPt.dYLoc, Z = minZ });
            corners.Add(new Point { X = endPt.dXLoc, Y = endPt.dYLoc, Z = maxZ });
            corners.Add(new Point { X = startPt.dXLoc, Y = startPt.dYLoc, Z = maxZ });
            corners.Add(new Point { X = startPt.dXLoc, Y = startPt.dYLoc, Z = minZ });

            // Create outline from corner points
            Polyline outline = new Polyline();
            outline.ControlPoints = corners;
            List<Polyline> outlines = new List<Polyline>();
            outlines.Add(outline);

            List<PanelPlanar> bhomPanels = Create.PanelPlanar(outlines);

            PanelPlanar bhomPanel = bhomPanels[0];

            HashSet<String> tag = new HashSet<string>();
            tag.Add("WallPanel");

            bhomPanel.Tags = tag;

            return bhomPanel;
        }

        public static PanelPlanar ToBHoMObject(IDeck IDeck, IModel IModel, int IStoryUID)
        {
            //Get all floor props
            ICompDeckProps ICompDeckProps = IModel.GetCompositeDeckProps();
            INonCompDeckProps INonCompDeckProps = IModel.GetNonCompDeckProps();
            IConcSlabProps IConcSlabProps = IModel.GetConcreteSlabProps();

            //Get panel props
            EDeckType type = IDeck.eDeckPropType;

            IProperty2D bh2DProp = null;
            ConstantThickness deck2DProp = new ConstantThickness();
            double deckThickness = 0;
            string deckLabel = "";
            int deckID = IDeck.lPropID;
            if (type == EDeckType.eDeckType_Composite)
            {
                ICompDeckProp DeckProp = ICompDeckProps.Get(deckID);
                deckThickness = DeckProp.dThickAboveFlutes;
                deckLabel = DeckProp.strLabel + deckThickness.ToString();
            }
            else if (type == EDeckType.eDeckType_Concrete)
            {
                IConcSlabProp DeckProp = IConcSlabProps.Get(deckID);
                deckThickness = DeckProp.dThickness;
                deckLabel = DeckProp.strLabel;
            }
            else if (type == EDeckType.eDeckType_NonComposite)
            {
                INonCompDeckProp DeckProp = INonCompDeckProps.Get(deckID);
                deckThickness = DeckProp.dEffectiveThickness;
                deckLabel = DeckProp.strLabel;
            }


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

            //Create panel per outline polylines
            List<Polyline> outlines = new List<Polyline>();
            outlines.Add(outline);
            List<PanelPlanar> bhomPanels = Create.PanelPlanar(outlines);
            //Create openings per openings polylines
            int numOpenings = openingPLs.Count();

            //Create openings
            List<Opening> bhomOpenings = new List<Opening>();
            for (int i = 0; i < numOpenings; i++)
            {
                Opening bhomOpening = Create.Opening(openingPLs[i]);
                bhomOpenings.Add(bhomOpening);
            }

            //Create panel and add attributes
            PanelPlanar bhomPanel = bhomPanels[0];
            bhomPanel.Openings = bhomOpenings;

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Floor");

            bhomPanel.Tags = tag;
            bhomPanel.Name = type.ToString();

            deck2DProp.Name = deckLabel;
            deck2DProp.Thickness = deckThickness;
            bhomPanel.Property = deck2DProp;

            return bhomPanel;
        }

        //Currently obsolete, use IWallPanel method instead; keeping for potential future use
        public static PanelPlanar ToBHoMObject(IWall IWall)
        {

            //Extract properties
            List<string> CustomProps = new List<string>();
            double thickness = IWall.dThickness;
            EFRAMETYPE type = IWall.eFramingType;
            EMATERIALTYPES material = IWall.eMaterial;

            //Find corner points of wall in RAM model
            SCoordinate TopstartPt = new SCoordinate();
            SCoordinate TopendPt = new SCoordinate();
            SCoordinate BottomstartPt = new SCoordinate();
            SCoordinate BottomendPt = new SCoordinate();

            IWall.GetEndCoordinates(ref TopstartPt, ref TopendPt, ref BottomstartPt, ref BottomendPt);

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
            List<Polyline> outlines = new List<Polyline>();
            outlines.Add(outline);

            List<PanelPlanar> bhomPanels = Create.PanelPlanar(outlines);

            PanelPlanar bhomPanel = bhomPanels[0];

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Wall");

            bhomPanel.Tags = tag;
            bhomPanel.Name = thickness.ToString() + " " + material.ToString();

            return bhomPanel;
        }

        public static Node ToBHoMObject(INode INode)
        {

            // Get the location of the node
            SCoordinate Location = new SCoordinate();
            Location = INode.sLocation;

            Node Node = new Node();

            Node.Position = new BH.oM.Geometry.Point() { X = Location.dXLoc, Y = Location.dYLoc, Z = Location.dZLoc };

            IDisplacements IDisplacements = INode.GetDisplacements();
            IMemberForces IMemberForces = INode.GetReactions();


            for (int i = 0; i < IDisplacements.GetCount(); i++)
            {
                IDisplacement IDisplacement = IDisplacements.GetAt(i);

                double x = IDisplacement.dDispX;
                double y = IDisplacement.dDispY;
                double z = IDisplacement.dDispZ;
                double thetax = IDisplacement.dThetaX;
                double thetay = IDisplacement.dThetaY;
                double thetaz = IDisplacement.dThetaZ;

                // Unique RAM ID
                Node.CustomData["lUID"] = INode.lUniqueID;

                Node.CustomData["dX"] = x;
                Node.CustomData["dY"] = y;
                Node.CustomData["dZ"] = z;
                Node.CustomData["dthetaX"] = thetax;
                Node.CustomData["dthetaY"] = thetay;
                Node.CustomData["dthetaZ"] = thetaz;

            }

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

        public static Loadcase ToBHoMObject(ILoadCase ILoadCase)
        {

            Loadcase Loadcase = new Loadcase();
            Loadcase.Name = ILoadCase.strTypeLabel;
            string LoadType = ILoadCase.eLoadType.ToString();
            Loadcase.CustomData.Add("Type", LoadType);

            return Loadcase;
        }

        public static Grid ToBHoMObject(IGridSystem IGridSystem, IModelGrid IModelGrid, int counter)
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

            //implement max grid length per bounds or max dCoord
            //GridLengthX = spacingY;
            //GridLengthY = spacingX;

            Point gridCoordPoint1 = new Point();
            Point gridCoordPoint2 = new Point();

            //check if what type is the GridSystem : orthogonal or radial ?? 
            Boolean gridIsOrtho = false;
            Boolean gridIsRadial = false;

            if (gridSystemType == "eGridOrthogonal")   // code to place grids in orthogonal X and Y
            {
                gridIsOrtho = true;
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
                gridIsRadial = true;
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



    } //Public Convert methods ends here 
}
