using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structural.Elements;
using BH.oM.Geometry;
using BH.oM.Structural.Loads;
using BH.Engine.Structure;
using BH.oM.Structural.Properties;
using RAMDATAACCESSLib;

namespace BH.Engine.RAM
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        //Add methods for converting to BHoM from the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static Node ToBHoM(this RAMNode node)
        //{
        //    //Insert code for conversion
        //}

        /***************************************************/

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

            return  bhomBar;
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

            // Get coordinates from IBeam
            SCoordinate startPt = new SCoordinate();
            SCoordinate endPt = new SCoordinate();
            IBeam.GetCoordinates(EBeamCoordLoc.eBeamEnds, ref startPt, ref endPt);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
            endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

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

            bhomBar.CustomData["Design Capacity Interaction"] = DCI;
            bhomBar.CustomData["Critical Deflection Interaction"] = CDI;

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

            return bhomBar;
        }

        public static Bar ToBHoMObject(IHorizBrace IHorizBrace, ILayoutHorizBrace ILayoutHorizBrace, double dElevation)
        {

            string section = IHorizBrace.strSectionLabel;

            // Get the start and end pts of every beam
            double StartSupportX = new double();
            double StartSupportY = new double();
            double StartSupportZOffset = new double();
            double EndSupportX = new double();
            double EndSupportY = new double();
            double EndSupportZOffset = new double();
            double StoryZ = dElevation;


            // Get coordinates from ILayout Beam
            ILayoutHorizBrace.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset };
            endNode.Position = new BH.oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset };

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            return bhomBar;
        }


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
            corners.Add(new Point { X = BottomendPt.dXLoc, Y = BottomendPt.dXLoc, Z = BottomendPt.dZLoc });
            corners.Add(new Point { X = BottomstartPt.dXLoc, Y = BottomstartPt.dYLoc, Z = BottomstartPt.dZLoc });
         
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

        //TODO - Create Convert Method for Floors

        //public static PanelPlanar ToBHoMObject(IDeck IDeck)
        //{

        //    //Extract properties
        //    EDeckType type = IDeck.eDeckPropType;

        //    //Find polylines of deck in RAM Model
        //    IDeck.GetPoints

        //    // Create outline from corner points
        //    Polyline outline = new Polyline();
        //    outline.ControlPoints = corners;
        //    List<Polyline> outlines = new List<Polyline>();
        //    outlines.Add(outline);

        //    List<PanelPlanar> bhomPanels = Create.PanelPlanar(outlines);

        //    PanelPlanar bhomPanel = bhomPanels[0];

        //    HashSet<String> tag = new HashSet<string>();
        //    tag.Add("Floor");

        //    bhomPanel.Tags = tag;
        //    bhomPanel.Name = type.ToString();

        //    return bhomPanel;
        //}

        public static Node ToBHoMObject(INode INode)
        {

            // Get the location of the node
            SCoordinate Location = new SCoordinate();
            Location = INode.sLocation;
           
            Node Node = new Node();
           
            Node.Position = new BH.oM.Geometry.Point() { X = Location.dXLoc, Y = Location.dYLoc, Z = Location.dZLoc };
            IDisplacements IDisplacements = INode.GetDisplacements();
            // IMemberForces IMemberForces = INode.GetReactions();
           

            for (int i = 0; i < IDisplacements.GetCount(); i++)
            {
                IDisplacement IDisplacement = IDisplacements.GetAt(i);

                double x = IDisplacement.dDispX;
                double y = IDisplacement.dDispY;
                double z = IDisplacement.dDispZ;
                double thetax = IDisplacement.dThetaX;
                double thetay = IDisplacement.dThetaY;
                double thetaz = IDisplacement.dThetaZ;

                Node.CustomData["dX"] = x;
                Node.CustomData["dY"] = y;
                Node.CustomData["dZ"] = z;
                Node.CustomData["dthetaX"] = thetax;
                Node.CustomData["dthetaY"] = thetay;
                Node.CustomData["dthetaZ"] = thetaz;

            }

            //for (int i = 0; i < IMemberForces.GetCount(); i++)
            //{
            //    IMemberForce IMemberForce = IMemberForces.GetAt(i);
            //    IMemberForce.ToString();
            //    Node.CustomData["Reaction" + i.ToString()] = IMemberForce.ToString();
            //}

            return Node;
        }

        public static Loadcase ToBHoMObject(ILoadCase ILoadCase)
        {

            Loadcase Loadcase = new Loadcase();
            Loadcase.Name = ILoadCase.strTypeLabel;

            return Loadcase;
        }


    }
}
