using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Properties;
using BH.oM.Structure.Properties.Section;
using BH.oM.Structure.Properties.Surface;
using BH.oM.Structure.Loads;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.oM.Architecture.Elements;


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

            // Register Floor types
            IFloorTypes IFloorTypes;
            IFloorType IFloorType;
            IStories IStories;
            IStory IStory;
            ILayoutColumns ILayoutColumns;
            ILayoutBeams ILayoutBeams;

            //Split nodes into beams and colummns
            List<Bar> columns = new List<Bar>();
            List<Bar> beams = new List<Bar>();
            List<double> beamHeights = new List<double>();
            List<double> levelHeights = new List<double>();

            //Create null section property
            ISectionProperty nullSectionProp = Engine.Structure.Create.SteelTubeSection(10, 1, null, "unassigned");

            // Find all level heights present
            foreach (Bar bar in bars)
            {
                //Assign null section property
                if (bar.SectionProperty == null)
                {
                    bar.SectionProperty = nullSectionProp;
                }

                if (bar.StartNode.Position.Z > bar.EndNode.Position.Z)
                {
                    Node LowNode = bar.EndNode;
                    Node HighNode = bar.StartNode;
                    bar.StartNode = LowNode;
                    bar.EndNode = HighNode;
                }

                double rise = bar.EndNode.Position.Z - bar.StartNode.Position.Z;
                double length = Engine.Structure.Query.Length(bar);
                double barRise = ((bar.EndNode.Position.Z - bar.StartNode.Position.Z) / Engine.Structure.Query.Length(bar));

                //if altitude>0 degrees create as column
                if (barRise>0.001)
                {
                    columns.Add(bar);
                    double zStart = bar.StartNode.Position.Z;
                    double zEnd = bar.EndNode.Position.Z;
                    beamHeights.Add(zStart);
                    beamHeights.Add(zEnd);
                    levelHeights.Add(Math.Round(zStart,0));
                    levelHeights.Add(Math.Round(zEnd,0));
                }
                else
                {
                    beams.Add(bar);
                    double z = bar.StartNode.Position.Z;
                    beamHeights.Add(z);
                    levelHeights.Add(Math.Round(z,0));
                }
            }

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Create new levels in RAM per unique z values
            CreateLevels(levelHeights,IModel);
            
            // Cycle through floortypes, access appropriate story, place beams on those stories
            IStories = IModel.GetStories();

            for (int i = 0; i < IStories.GetCount(); i++)
            {
                IStory = IStories.GetAt(i);
                IFloorType = IStory.GetFloorType();
                ILayoutBeams = IFloorType.GetLayoutBeams();
                ILayoutColumns = IFloorType.GetLayoutColumns();


                //Cycle through bars; if z of bar = the floor height, add it
                for (int j = 0; j < beams.Count(); j++)
                {
                    
                    Bar bar = beams[j];

                    double xStart = bar.StartNode.Position.X;
                    double yStart = bar.StartNode.Position.Y;
                    double xEnd = bar.EndNode.Position.X;
                    double yEnd = bar.EndNode.Position.Y;
                    double zStart = Math.Round(bar.StartNode.Position.Z,0);
                    double zEnd = Math.Round(bar.EndNode.Position.Z,0);

                    //If bar is on level, add it during that iteration of the loop 
                    if (zStart == IStory.dElevation)
                    {
                        ILayoutBeam ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, xStart, yStart, 0, xEnd, yEnd,5);
                        ILayoutBeam.strSectionLabel = Engine.RAM.Convert.ToRAM(bar.SectionProperty.Name);
                    }
                }

                //Cycle through columns; if z of column = the floor height, add it
                for (int j = 0; j < columns.Count(); j++)
                {
                    //If bar is on level, add it during that iteration of the loop 
                    Bar bar = columns[j];
                    

                    double xStart = bar.StartNode.Position.X;
                    double yStart = bar.StartNode.Position.Y;
                    double zStart = Math.Round(bar.StartNode.Position.Z,0);
                    double xEnd = bar.EndNode.Position.X;
                    double yEnd = bar.EndNode.Position.Y;
                    double zEnd = Math.Round(bar.EndNode.Position.Z,0);

                    if (zEnd == IStory.dElevation)
                    {
                        IFloorType = IStory.GetFloorType();
                        ILayoutColumns = IFloorType.GetLayoutColumns();

                        if (Engine.Structure.Query.IsVertical(bar))
                        {
                            //Failing if no section property is provided
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add(Engine.RAM.Convert.ToRAM(bar.SectionProperty.Material), xEnd, yEnd, 0, 0);
                            ILayoutColumn.strSectionLabel = Engine.RAM.Convert.ToRAM(bar.SectionProperty.Name);
                        }
                        else
                        {
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add2(Engine.RAM.Convert.ToRAM(bar.SectionProperty.Material), xEnd, yEnd, xStart, yStart, 0, 0);
                            ILayoutColumn.strSectionLabel = Engine.RAM.Convert.ToRAM(bar.SectionProperty.Name);
                        }
                        
                    }
                }

            }


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////


            //foreach (Bar bar in bars)
            //{
            //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
            //    object barId = bar.CustomData[AdapterId];
            //    //If also the default implmentation for the DependencyTypes is used,
            //    //one can from here get the id's of the subobjects by calling (cast into applicable type used by the software): 
            //    object startNodeId = bar.StartNode.CustomData[AdapterId];
            //    object endNodeId = bar.EndNode.CustomData[AdapterId];
            //    object SecPropId = bar.SectionProperty.CustomData[AdapterId];
            //}

            //Save file
            RAMDataAccIDBIO.SaveDatabase();

            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            //System.IO.File.Delete(filePathUserfile);
            return true;
        }

        /***************************************************/


        private bool CreateCollection(IEnumerable<Node> nodes)
        {
            //Code for creating a collection of nodes in the software
          
            foreach (Node node in nodes)
            {
                //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
                object nodeId = node.CustomData[AdapterId];
            }

            throw new NotImplementedException();
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<ISectionProperty> sectionProperties)
        {
            //Code for creating a collection of section properties in the software


            return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Material> materials)
        {
            //Code for creating a collection of materials in the software


           return true;
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<ISurfaceProperty> ISurfacePropertys)
        {           
            //TODO: DECK PROPERTY FUNCTIONALITY
            
            ////Code for creating a collection of deck properties in the software (DEFAULT FOR NOW)

            ////Access model
            //IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ////Get composite deck properties
            //ICompDeckProps ICompDeckProps = IModel.GetCompositeDeckProps();

            //foreach (ISurfaceProperty iProp in ISurfacePropertys)
            //{
            //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
            //    string deckName = iProp.Name;
            //    double thickness = 6;
            //    double studLength = 4;

            //    ICompDeckProps.Add(deckName, thickness, studLength);

            //    object iPropId = iProp.CustomData[AdapterId];
            //}

            return true;
        }

        //TODO: TEST METHOD AND RESOLVE FREEZING WHEN OPENING RAM

        // Create Panel Method -- Commented because it is not yet working
        private bool CreateCollection(IEnumerable<PanelPlanar> bhomPanels)
        {
            //Code for creating a collection of floors (and/or walls?) in the software

            List<PanelPlanar> panels = bhomPanels.ToList();

            // Register Floor types
            IFloorTypes IFloorTypes;
            IFloorType IFloorType;
            IStories IStories;
            IStory IStory;

            //Create wall and floor lists with individual heights
            List<PanelPlanar> wallPanels = new List<PanelPlanar>();
            List<PanelPlanar> floors = new List<PanelPlanar>();
            List<double> panelHeights = new List<double>();
            List<Point> panelPoints = new List<Point>();

            // Split walls and floors
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

            CreateLevels(panelHeights, IModel);

            IStories = IModel.GetStories();

            //Get concrete deck properties
            IConcSlabProps IConcSlabProps = IModel.GetConcreteSlabProps();
            IConcSlabProp IConcSlabProp;

            // Cycle through floortypes, access appropriate story, place panels on those stories
            for (int i = 0; i < IStories.GetCount(); i++)
            {

                IStory = IStories.GetAt(i);
                IFloorType = IStory.GetFloorType();

                //Cycle through floors; if z of bar = the floor height, add it
                for (int j = 0; j < floors.Count(); j++)
                {

                    PanelPlanar floor = floors[j];

                    // Find outline of planar panel
                    PolyCurve outline = BH.Engine.Structure.Query.Outline(floor);


                    // Get coords of corner points
                    List<Point> ctrlPoints = new List<Point>();
                    ctrlPoints = BH.Engine.Geometry.Query.ControlPoints(outline);


                    // Get list of coordinates
                    List<SCoordinate> corners = new List<SCoordinate>();

                    foreach (Point point in ctrlPoints)
                    {
                        SCoordinate corner = BH.Engine.RAM.Convert.ToRAM(point);
                        corners.Add(corner);
                    }

                    // If on level, add deck to IDecks for that level
                    if (Math.Round(corners[0].dZLoc,0) == IStory.dElevation)
                    {

                        IDecks IDecks = IFloorType.GetDecks();
                        IDeck IDeck = null;

                        // Set slab edges (required in addition to deck perimeter
                        ISlabEdges ISlabEdges = IFloorType.GetAllSlabEdges();

                        for (int k = 0; k < corners.Count - 1; k++)
                        {
                            ISlabEdges.Add(corners[k].dXLoc, corners[k].dYLoc, corners[k + 1].dXLoc, corners[k + 1].dYLoc, 0);
                        }

                        //// Default panel properties to apply to model
                        //string deckName = "Default RAM_Toolkit"; //pull deck name from decktable
                        //double thickness = 8;
                        //double selfweight = 150;

                        //// Create Deck (IDecks.Add causes RAMDataAccIDBIO to be read only causing crash, slab edges only for now) 
                        //IConcSlabProp = IConcSlabProps.Add(deckName, thickness, selfweight);
                        //IDeck = IDecks.Add(IConcSlabProp.lUID, ctrlPoints.Count); // This causes the read memory error crashing at save
                        //IPoints IPoints = IDeck.GetPoints();

                        //for (int k = 0; k < corners.Count; k++)
                        //{
                        //    IPoints.Delete(k);
                        //    IPoints.InsertAt(k, corners[k]);
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
                    BoundingBox wallBounds = Query.Bounds(outline);
                    Point wallMin = wallBounds.Min;
                    Point wallMax = wallBounds.Max;

                    // If on level, add deck to IDecks for that level
                    if (Math.Round(wallMax.Z, 0) == IStory.dElevation)
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

        private bool CreateLevels(List<double> Elevations, IModel IModel)
        {

            Elevations.Sort();

            List<double> levelHeights = Elevations.Distinct().ToList();

            //RAM requires positive levels. Added logic allows for throwing negative level exception.

            if (levelHeights[0] < 0)
            {
                throw new Exception("Base level can not be negative for RAM. Please move model origin point to set all geometry and levels at 0 or greater.");
            }

            // Register Floor types
            IFloorTypes IFloorTypes;
            IFloorType IFloorType;
            IStories IStories;
            IStory IStory;
            List<double> levelHeightsInRam = new List<double>();
            List<double> allUniqueLevels = new List<double>();

            // Get all levels already in RAM
            IStories = IModel.GetStories();
            double storyCount = IStories.GetCount();
            for (int i = 0; i < storyCount; i++)
            {
                IStory = IStories.GetAt(i);
                double elev = IStory.dElevation;
                levelHeightsInRam.Add(elev);
            }

            levelHeights.AddRange(levelHeightsInRam);
            levelHeights.Sort();

            List<double> sortedLevelHeights = levelHeights.Distinct().ToList();


            //Create floor type at each level

            for (int i = 0; i < sortedLevelHeights.Count(); i++)
            {
                string LevelName = "Level " + sortedLevelHeights[i].ToString();
                string StoryName = "Story " + i.ToString();

                // Find floor heights from z-elevations
                double height;
                // Ground floor ht = 0 for RAM
                if (i == 0) { height = sortedLevelHeights[i]; }
                else { height = sortedLevelHeights[i] - sortedLevelHeights[i - 1]; }

                IStories = IModel.GetStories();

                if (!levelHeightsInRam.Contains(sortedLevelHeights[i]))
                {
                    IFloorTypes = IModel.GetFloorTypes();
                    IFloorType = IFloorTypes.Add(LevelName);

                    // Insert story at index
                    IStories.InsertAt(i,IFloorType.lUID, StoryName, height);
                }
                else
                {
                    //Set story and floor type data to sync with added levels
                    IStory = IStories.GetAt(i);
                    IStory.dFlrHeight = height;
                    IStory.strLabel = StoryName;
                    IFloorType = IStory.GetFloorType();
                    IFloorType.strLabel = LevelName;
                }
                

            }
            return true;
        }


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
            double gridOffsetX = 0;
            double gridOffsetY = 0;
            IGridSystem IGridSystemXY = null;
            IGridSystem IGridSystemRad = null;
            IGridSystem IGridSystemSk = null;
            IModelGrids IModelGridsXY = null;
            IModelGrids IModelGridsRad = null;
            IModelGrids IModelGridsSk = null;



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
                    //check if the first provided line are offset from the 0,0,0 and give that as gridOffset in X and Y
                    //add lines to corresponding lists (XGrids, YGrids) based on their  orientation
                    if (gridLine.StartPoint().X == gridLine.EndPoint().X)
                    {
                        YGrids.Add(grid);
                    }
                    else if (gridLine.StartPoint().Y == gridLine.EndPoint().Y)
                    {
                        XGrids.Add(grid);
                    }
                    else
                    {
                        skewGrids.Add(grid);
                    }

                }
                if (i == 0)
                {
                    if (gridLine.StartPoint().X != 0)
                    {
                        gridOffsetX = gridLine.StartPoint().X;

                    }
                    if (gridLine.StartPoint().Y != 0)
                    {
                        gridOffsetY = gridLine.StartPoint().Y;

                    }
                }
            }


            //Create grid systems per grid lists

            //XYGrids
            if (YGrids.Count() != 0 || XGrids.Count() != 0)
            {
                 gridSystemLabel = "XY_grid";
                 IGridSystemXY = IGridSystems.Add(gridSystemLabel);
                 IGridSystemXY.dXOffset = gridOffsetX;
                 IGridSystemXY.dYOffset = gridOffsetY;
                 IGridSystemXY.eOrientationType = SGridSysType.eGridOrthogonal;
                 IGridSystemXY.dRotation = gridSystemRotation;
                 IModelGridsXY = IGridSystemXY.GetGrids();
            }



            //Radial Circular Grid
            if (circGrids.Count() != 0)
            {
                gridSystemLabel = "Radial_grid";
                IGridSystemRad = IGridSystems.Add(gridSystemLabel);
                IGridSystemRad.dXOffset = gridOffsetX;
                IGridSystemRad.dYOffset = gridOffsetY;
                IGridSystemRad.eOrientationType = SGridSysType.eGridRadial;
                IGridSystemRad.dRotation = gridSystemRotation;
                IModelGridsRad = IGridSystemRad.GetGrids();
            }
            // Skewed grid
            if (skewGrids.Count() != 0) {
                gridSystemLabel = "Skew_gird";
                IGridSystemSk = IGridSystems.Add(gridSystemLabel);
                IGridSystemSk.dXOffset = 0;
                IGridSystemSk.dYOffset = 0;
                IGridSystemSk.eOrientationType = SGridSysType.eGridSkewed;
                IGridSystemSk.dRotation = gridSystemRotation;
                IModelGridsSk = IGridSystemSk.GetGrids();

            }




            //labels for grids in each direction
            string gridLabelX = "X";
            string gridLabelY = "Y";
            int gridCountX = 1;
            int gridCountY = 1;

            foreach (Grid XGrid in XGrids)
            {

                XGrid.Name = gridLabelX + gridCountX.ToString();
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(XGrid.Curve as dynamic, 10);
                IModelGridsXY.Add(XGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y);
                //IGridSystemXY = Engine.RAM.Convert.ToRAM(XGrid, IModelGridsXY, IGridSystemXY);
                gridCountX += 1;
            }

            foreach (Grid YGrid in YGrids)
            {
                YGrid.Name = gridLabelY + gridCountY.ToString();
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(YGrid.Curve as dynamic, 10);

                IModelGridsXY.Add(YGrid.Name, EGridAxis.eGridXorRadialAxis, gridLine.StartPoint().X);
                //IGridSystemXY = Engine.RAM.Convert.ToRAM(YGrid, IModelGridsXY, IGridSystemXY);
                gridCountY += 1;
            }

            foreach (Grid cGrid in circGrids)
            {

                IModelGridsRad.Add(cGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y);
                // TODO: add code to impement circular grids
                // Create GridSystem in RAM for each unique centerpt of circGrids  

            }

            foreach (Grid skGrid in skewGrids)
            {
                // TODO: add code to impement skewed grids
                // Create GridSystem in RAM for each unique angle of skewGrids

            }

            //call the convert method 
            IGridSystemXY = Engine.RAM.Convert.ToRAM(Grids, IModelGridsXY, IGridSystemXY);

            //get the ID of the fridsystem
            int gridSystemID = IGridSystemXY.lUID;


            /* FOR now we are not creating not creating floor type up until we test the rest of the elements
            //TODO: NEEDS TO BE TESTED
            // Create a default floor type and assign the newly created gridsystem
            //string defFloorTypeName = "Default_floorType";
            //IFloorType myFloorType = myFloorTypes.Add(defFloorTypeName);
            //IStories myStories = IModel.GetStories();


            //Cycle through floortypes, access the existing floortype/story, place grids on those stories
            for (int i = 0; i < myFloorTypes.GetCount(); i++)
                {
                    myFloorType = myFloorTypes.GetAt(i);
                    IStory myStory= myStories.GetAt(i);
                    DAArray gsID = myFloorType.GetGridSystemIDArray();
                    gsID.Add(IGridSystemXY.lUID, 0);
                    myFloorType.SetGridSystemIDArray(gsID);
                }
                */



            //Save file
            RAMDataAccIDBIO.SaveDatabase();
            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            return true;
        }

        /***************************************************/
    }
}
