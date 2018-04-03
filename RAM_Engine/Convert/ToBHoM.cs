using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structural.Elements;
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
        //    //Insert code for convertion
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

            //// Get Steel beam results **STILL IN PROGRESS
            //ISteelBeamDesignResult Result = IBeam.GetSteelDesignResult();
            //DAArray ppalNumStuds = Result.GetNumStudsInSegments();
            //int numStudSegments = new int();
            //ppalNumStuds.GetSize(ref numStudSegments);
            //double Camber = IBeam.dCamber;

            // Get the start and end pts of every beam
            double StartSupportX = new double();
            double StartSupportY = new double();
            double StartSupportZOffset = new double();
            double EndSupportX = new double();
            double EndSupportY = new double();
            double EndSupportZOffset = new double();
            double StoryZ = dElevation;
         

            // Get coordinates from ILayout Beam
            ILayoutBeam.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
            Node startNode = new Node();
            Node endNode = new Node();
            startNode.Position = new BH.oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset };
            endNode.Position = new BH.oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset };

            Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = section };

            bhomBar.OrientationAngle = 0;

            //// Add studs to custom Data  *STILL IN PROGRESS, gives "specific cast is not valid" error through Grasshopper when turned on
            //for (int i = 0; i < numStudSegments; i++)
            //{
            //    var pvrtItem = new object();
            //    string numStuds = ppalNumStuds.GetAt(i, pvrtItem).ToString();

            //    //Check to make sure studs are set
            //    if ( (bool) pvrtItem)
            //    {
            //        bhomBar.CustomData["Studs" + i.ToString()] = numStuds;
            //    }
            //}

            //// Add camber to Custom Data
            //if (Camber > Double.MinValue)
            //{
            //    bhomBar.CustomData["Camber"] = Camber;
            //    //bhomBar.CustomData.Add("Camber", Camber);
            //}

            //bhomBar.CustomData["Test1"] = "Object1";
            //bhomBar.CustomData.Add("Test1", "Object1_2");
            //bhomBar.CustomData["Test2"] = "Object2";
            //bhomBar.CustomData.Add("Test2", "Object2_2");

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

            List<string> CustomProps = new List<string>();
            double thickness = IWall.dThickness;
            EFRAMETYPE type = IWall.eFramingType;
            EMATERIALTYPES material = IWall.eMaterial;

            // Get coordinates from ILayout Beam
            PanelPlanar bhomPanel = new PanelPlanar();

            SCoordinate TopstartPt = new SCoordinate();
            SCoordinate TopendPt = new SCoordinate();
            SCoordinate BottomstartPt = new SCoordinate();
            SCoordinate BottomendPt = new SCoordinate();

            IWall.GetEndCoordinates(ref TopstartPt, ref TopendPt, ref BottomstartPt, ref BottomendPt);

            HashSet<String> tag = new HashSet<string>();
            tag.Add("Wall");

            bhomPanel.Tags = tag;
            bhomPanel.Name = thickness.ToString() + " " + material.ToString();

            return bhomPanel;
        }


    }
}
