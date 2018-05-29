using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Structural.Loads;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            bool success = true;        //boolean returning if the creation was successfull or not

            //if (objects.First() is Bar)
            //{
            //    success = CreateCollection(objects as IEnumerable<Bar>);
            //}

            //if (objects.First() is PanelPlanar)
            //{
            //    success = CreateCollection(objects as IEnumerable<PanelPlanar>);
            //}


            // Commented out to just read Bar for Testing
            success = CreateCollection(objects as dynamic);

            //UpdateViews()             //If there exists a command for updating the views is the software call it now:

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
            ILayoutColumns ILayoutColumns;
            ILayoutBeams ILayoutBeams;

            //Split nodes into beams and colummns
            List<Bar> columns = new List<Bar>();
            List<Bar> beams = new List<Bar>();
            List<double> beamHeights = new List<double>();
            List<double> levelHeights = new List<double>();

            // Find all level heights present
            foreach (Bar bar in bars)
            {
                if (BH.Engine.Structure.Query.IsVertical(bar))
                {
                    columns.Add(bar);
                }
                else
                {
                    beams.Add(bar);
                    double z = bar.StartNode.Position.Z;
                    double zRounded = Math.Round(z);
                    beamHeights.Add(z);
                    levelHeights.Add(zRounded);
                }
            }

            levelHeights.Sort();
            List<double> levelHeightsUnique = levelHeights.Distinct().ToList();

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            

            //Create floor type at each level
            for (int i = 0; i < levelHeightsUnique.Count(); i++)
            {

                string LevelName = "Level " + levelHeightsUnique[i].ToString();
                string StoryName = "Story " + i.ToString();

                IFloorTypes = IModel.GetFloorTypes();

                IFloorTypes.Add(LevelName);
                IFloorType = IFloorTypes.GetAt(i);

                ILayoutColumns = IFloorType.GetLayoutColumns();
                ILayoutBeams = IFloorType.GetLayoutBeams();

                IStories = IModel.GetStories();

                // Find floor heights from z-elevations
                double height;
                if (i == 0) { height = levelHeightsUnique[i]; }
                else { height = levelHeightsUnique[i] - levelHeightsUnique[i - 1]; }

                IStories.Add(IFloorType.lUID, StoryName, height * 12);

            }

            // Cycle through floortypes, access appropriate story, place beams on those stories
            for (int i = 0; i < levelHeightsUnique.Count(); i++)
            {

                IFloorTypes = IModel.GetFloorTypes();
                IFloorType = IFloorTypes.GetAt(i);
                ILayoutBeams = IFloorType.GetLayoutBeams();
                ILayoutColumns = IFloorType.GetLayoutColumns();

                //Cycle through bars; if z of bar = the floor height, add it
                for (int j = 0; j < beams.Count(); j++)
                {
                    //If bar is on level, add it during that iteration of the loop 
                    Bar bar = beams[j];

                    double xStart = bar.StartNode.Position.X * 12;
                    double yStart = bar.StartNode.Position.Y * 12;
                    double xEnd = bar.EndNode.Position.X * 12;
                    double yEnd = bar.EndNode.Position.Y * 12;

                    if (Math.Round(bar.StartNode.Position.Z) == levelHeightsUnique[i])
                    {
                        ILayoutBeam ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, xStart, yStart, 0, xEnd, yEnd, 0);
                        //ILayoutBeam.strSectionLabel = bar.SectionProperty.Name;
                        ILayoutBeam.strSectionLabel = bar.Name; // for debugging, checking scale
                    }
                }

                //Cycle through columns; if z of column = the floor height, add it
                for (int j = 0; j < columns.Count(); j++)
                {
                    //If bar is on level, add it during that iteration of the loop 
                    Bar bar = columns[j];

                    double xStart = bar.StartNode.Position.X * 12;
                    double yStart = bar.StartNode.Position.Y * 12;
                    double zStart = bar.StartNode.Position.Z * 12;
                    double xEnd = bar.EndNode.Position.X * 12;
                    double yEnd = bar.EndNode.Position.Y * 12;
                    double zEnd = bar.EndNode.Position.Z * 12;

                    if (bar.StartNode.Position.Z == levelHeights[i])
                    {
                        if (zStart <= zEnd)
                        {
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add(BH.Engine.RAM.Convert.ToRAM(bar.SectionProperty.Material), xStart, yStart, 0, 0);
                            ILayoutColumn.strSectionLabel = bar.Name; // for debugging, checking scale
                        }
                        else
                        {
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add(BH.Engine.RAM.Convert.ToRAM(bar.SectionProperty.Material), xEnd, yEnd, 0, 0);
                            ILayoutColumn.strSectionLabel = bar.Name; // for debugging, checking scale
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

        private bool CreateCollection(IEnumerable<IProperty2D> IProperty2Ds)
        {
            ////Code for creating a collection of deck properties in the software (DEFAULT FOR NOW)

            ////Access model
            //IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            //IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ////Get composite deck properties
            //ICompDeckProps ICompDeckProps = IModel.GetCompositeDeckProps();

            //foreach (IProperty2D iProp in IProperty2Ds)
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


        private bool CreateCollection(IEnumerable<PanelPlanar> bhomPanels)
        {
            throw new NotImplementedException();
        }

        //// Create Panel Method -- Commented because it is not yet working
        //private bool CreateCollection(IEnumerable<PanelPlanar> bhomPanels)
        //{
        //    //Code for creating a collection of floors (and/or walls?) in the software

        //    List<PanelPlanar> panels = bhomPanels.ToList();

        //    // Register Floor types
        //    IFloorTypes IFloorTypes;
        //    IFloorType IFloorType;
        //    IStories IStories;
        //    IStory IStory;

        //    //Split nodes into beams and colummns
        //    List<PanelPlanar> WallPanels = new List<PanelPlanar>();
        //    List<PanelPlanar> floors = new List<PanelPlanar>();
        //    List<double> panelHeights = new List<double>();


        //    // Find all level heights present
        //    foreach (PanelPlanar panel in panels)
        //    {
        //        if (panel.Tags.Contains("WallPanel"))
        //        {
        //            WallPanels.Add(panel);
        //        }
        //        else
        //        {
        //            floors.Add(panel);
        //        }
        //    }

        //    //Access model
        //    IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
        //    IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

        //    IFloorTypes = IModel.GetFloorTypes();
        //    IStories = IModel.GetStories();


        //    // Cycle through floortypes, access appropriate story, place beams on those stories
        //    for (int i = 0; i < IFloorTypes.GetCount(); i++)
        //    {

        //        IFloorType = IFloorTypes.GetAt(i);
        //        IStory = IStories.GetAt(i);

        //        IDecks IDecks = IFloorType.GetDecks();
        //        IDeck IDeck;

        //        //Cycle through bars; if z of bar = the floor height, add it
        //        for (int j = 0; j < floors.Count(); j++)
        //        {

        //            PanelPlanar floor = floors[j];

        //            // Find outline of planar panel
        //            List<SCoordinate> corners = new List<SCoordinate>();
        //            //Polyline outline = BH.Engine.Structure.Query.Outline(floor);

        //            List<Edge> edges = floor.ExternalEdges;

        //            /////////////// Method 1 to get points
        //            //List<Point> ctrlPoints = edges.Select(e => (e.Curve as Line).Start)..ToList();

        //            /////////////// Method 2 to get points
        //            //List<ICurve> ICurveSegs = new List<ICurve>();
        //            //IEnumerable<List<ICurve>> segments = edges.Select(e => (e.Curve as PolyCurve).Curves.ToList<ICurve>());
        //            //List<Point> ctrlPoints2 = new List<Point>();

        //            //foreach (List<ICurve> segment in segments)
        //            //{
        //            //    for (int l = 0; l < segments.Count(); l++)
        //            //    {
        //            //        ICurveSegs.Add(segment[l]);
        //            //    }
        //            //}

        //            //for (int l = 0; l < segments.Count(); l++)
        //            //{
        //            //    Line bhomLine = ICurveSegs[l] as Line;
        //            //    ctrlPoints2.Add(bhomLine.Start);       
        //            //}

        //            ////////////// Method 3
        //            List<ICurve> segments2 = BH.Engine.Structure.Query.AllEdgeCurves(floor);
        //            List<Point> ctrlPoints3 = new List<Point>();
        //            PolyCurve bhomCurve = new PolyCurve { Curves = segments2.ToList() };

        //            ctrlPoints3 = BH.Engine.Geometry.Query.ControlPoints(bhomCurve);


        //            // Get list of coordinates
        //            foreach (Point point in ctrlPoints3)
        //            {
        //                SCoordinate corner = BH.Engine.RAM.Convert.ToRAM(point);
        //                corners.Add(corner);
        //            }

        //            //Add back first point to end
        //            corners.Add(corners[0]);

        //            // If on level, add deck to IDecks for that level
        //            if (Math.Round(corners[0].dZLoc) == IStory.dFlrHeight || Math.Round(corners[0].dZLoc) == IStory.dElevation)
        //            {

        //                IDeck = IDecks.Add(0, ctrlPoints3.Count + 1);
        //                IPoints IPoints = IDeck.GetPoints();

        //                for (int k = 0; k < corners.Count; k++)
        //                {
        //                    IPoints.Add(corners[k]);
        //                }

        //                IDeck.SetPoints(IPoints);

        //            }
        //        }

        //    }

        //    //Save file
        //    RAMDataAccIDBIO.SaveDatabase();

        //    // Release main interface and delete user file
        //    RAMDataAccIDBIO = null;
        //    //System.IO.File.Delete(filePathUserfile);

        //    return true;
        //}


        /***************************************************/
    }
}
