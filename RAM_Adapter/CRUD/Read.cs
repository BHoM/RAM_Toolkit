using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            //Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            if (type == typeof(Node))
                return ReadNodes(ids as dynamic);
            else if (type == typeof(Bar))
                return ReadBars(ids as dynamic);
            else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
                return ReadSectionProperties(ids as dynamic);
            else if (type == typeof(Material))
                return ReadMaterials(ids as dynamic);

            return null;
        }

        /***************************************************/
        /**** Private specific read methods             ****/
        /***************************************************/

        //The List<string> in the methods below can be changed to a list of any type of identification more suitable for the toolkit

        private List<Bar> ReadBars(List<string> ids = null)
        {
            //Implement code for reading bars
            
            
            // ***Should return list of "Bar", just testing with strings and endpoints

            //Read Columns ---------------------------------
            
            List<string> ColumnSections = new List<string>();
            List<Node> startNodes = new List<Node>();
            List<Node> endNodes = new List<Node>();
            List<Bar> bhomBars = new List<Bar>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            // Get stories
            IStories IStories = IModel.GetStories();
            int numStories = IStories.GetCount();

            // Get columns on each story
            for (int i = 0; i < numStories; i++)
            {
                IColumns IColumns = IStories.GetAt(i).GetColumns();
                int numColumns = IColumns.GetCount();

                // Find props of every column
                for (int j = 0; j < IColumns.GetCount(); j++)
                {

                    // Get the name of every column
                    IColumn IColumn = IColumns.GetAt(j);
                    string section = IColumn.strSectionLabel;
                    ColumnSections.Add(section);
                    // Get the start and end pts of every column
                    SCoordinate startPt = new SCoordinate();
                    SCoordinate endPt = new SCoordinate();
                    IColumn.GetEndCoordinates(ref startPt, ref endPt);
                    Node startNode = new Node();
                    Node endNode = new Node();
                    startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
                    endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };
                    startNodes.Add(startNode);
                    endNodes.Add(endNode);
                }

            }

            for (int i = 0; i < ColumnSections.Count(); i++)
            {
                //Node startNode = null; 
                //Node endNode = null; 
                Bar bhomBar = new Bar { StartNode = startNodes[i], EndNode = endNodes[i], Name = ColumnSections[i] };

                bhomBar.OrientationAngle = 0;

                bhomBars.Add(bhomBar);

            }


            //Read Beams ---------------------------------

            List<string> BeamSections = new List<string>();
            List<Node> startNodesBeams = new List<Node>();
            List<Node> endNodesBeams = new List<Node>();

            // Get beams on each story
            for (int i = 0; i < numStories; i++)
            {
                // Need both Beams and LayoutBeams, which is where the coordinates are stored; no z-value in Layout Beams, since they can be associated with multiple floors; have to add z manually
                ILayoutBeams ILayoutBeams = IStories.GetAt(i).GetFloorType().GetLayoutBeams();
                IBeams IBeams = IStories.GetAt(i).GetBeams();
                int numBeams = ILayoutBeams.GetCount();

                // Find props of every beam
                for (int j = 0; j < IBeams.GetCount(); j++)
                {

                    // Get the name of every beam
                    IBeam IBeam = IBeams.GetAt(j);
                    ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(j);
                    string section = IBeam.strSectionLabel;
                    BeamSections.Add(section);
                    // Get the start and end pts of every beam
                    double StartSupportX = new double();
                    double StartSupportY = new double();
                    double StartSupportZOffset = new double();
                    double EndSupportX = new double();
                    double EndSupportY = new double();
                    double EndSupportZOffset = new double();
                    double StoryZ = IStories.GetAt(i).dElevation;


                    // Get coordinates from ILayout Beam
                    ILayoutBeam.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
                    Node startNode = new Node();
                    Node endNode = new Node();
                    startNode.Position = new BH.oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset };
                    endNode.Position = new BH.oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset };
                    startNodesBeams.Add(startNode);
                    endNodesBeams.Add(endNode);
                }

            }

            for (int i = 0; i < BeamSections.Count(); i++)
            {
                //Node startNode = null; 
                //Node endNode = null; 
                Bar bhomBar = new Bar { StartNode = startNodesBeams[i], EndNode = endNodesBeams[i], Name = BeamSections[i] };

                bhomBar.OrientationAngle = 0;

                bhomBars.Add(bhomBar);

            }


            //Read Vertical Braces ---------------------------------

            List<string> VBraceSections = new List<string>();
            List<Node> startNodesVBrace = new List<Node>();
            List<Node> endNodesVBrace = new List<Node>();
 

            // Get Vertical Braces on each story
            for (int i = 0; i < numStories; i++)
            {
                IVerticalBraces IVBraces = IStories.GetAt(i).GetVerticalBraces();
                int numVBraces = IVBraces.GetCount();

                // Find props of every VBrace
                for (int j = 0; j < numVBraces; j++)
                {

                    // Get the name of every VBrace
                    IVerticalBrace IVBrace = IVBraces.GetAt(j);
                    string section = IVBrace.strSectionLabel;
                    VBraceSections.Add(section);
                    // Get the start and end pts of every VBrace
                    SCoordinate startPt = new SCoordinate();
                    SCoordinate endPt = new SCoordinate();
                    IVBrace.GetEndCoordinates(ref startPt, ref endPt);
                    Node startNode = new Node();
                    Node endNode = new Node();
                    startNode.Position = new BH.oM.Geometry.Point() { X = startPt.dXLoc, Y = startPt.dYLoc, Z = startPt.dZLoc };
                    endNode.Position = new BH.oM.Geometry.Point() { X = endPt.dXLoc, Y = endPt.dYLoc, Z = endPt.dZLoc };
                    startNodes.Add(startNode);
                    endNodes.Add(endNode);
                }

            }

            for (int i = 0; i < VBraceSections.Count(); i++)
            {
                //Node startNode = null; 
                //Node endNode = null; 
                Bar bhomBar = new Bar { StartNode = startNodes[i], EndNode = endNodes[i], Name = VBraceSections[i] };

                bhomBar.OrientationAngle = 0;

                bhomBars.Add(bhomBar);

            }



            //Read Horizontal Braces ---------------------------------

            List<string> HBraceSections = new List<string>();
            List<Node> startNodesHBraces = new List<Node>();
            List<Node> endNodesHBraces = new List<Node>();

            // Get HBrace on each story
            for (int i = 0; i < numStories; i++)
            {
                // Like beams, need both ILayout and Regular
                ILayoutHorizBraces ILayoutHorizBraces = IStories.GetAt(i).GetFloorType().GetLayoutHorizBraces();
                IHorizBraces IHorizBraces = IStories.GetAt(i).GetHorizBraces();
                int numBraces = ILayoutHorizBraces.GetCount();

                // Find props of every HBrace
                for (int j = 0; j < IHorizBraces.GetCount(); j++)
                {

                    // Get the name of every HBrace
                    IHorizBrace IHorizBrace = IHorizBraces.GetAt(j);
                    ILayoutHorizBrace ILayoutHorizBrace = ILayoutHorizBraces.GetAt(j);
                    string section = IHorizBrace.strSectionLabel;
                    HBraceSections.Add(section);
                    // Get the start and end pts of every HBrace
                    double StartSupportX = new double();
                    double StartSupportY = new double();
                    double StartSupportZOffset = new double();
                    double EndSupportX = new double();
                    double EndSupportY = new double();
                    double EndSupportZOffset = new double();
                    double StoryZ = IStories.GetAt(i).dElevation;


                    // Get coordinates from ILayoutHorizBrace
                    ILayoutHorizBrace.GetLayoutCoordinates(out StartSupportX, out StartSupportY, out StartSupportZOffset, out EndSupportX, out EndSupportY, out EndSupportZOffset);
                    Node startNode = new Node();
                    Node endNode = new Node();
                    startNode.Position = new BH.oM.Geometry.Point() { X = StartSupportX, Y = StartSupportY, Z = StoryZ + StartSupportZOffset };
                    endNode.Position = new BH.oM.Geometry.Point() { X = EndSupportX, Y = EndSupportY, Z = StoryZ + EndSupportZOffset };
                    startNodesHBraces.Add(startNode);
                    endNodesHBraces.Add(endNode);
                }

            }

            for (int i = 0; i < HBraceSections.Count(); i++)
            {
                //Node startNode = null; 
                //Node endNode = null; 
                Bar bhomBar = new Bar { StartNode = startNodesHBraces[i], EndNode = endNodesHBraces[i], Name = HBraceSections[i] };

                bhomBar.OrientationAngle = 0;

                bhomBars.Add(bhomBar);

            }







            return bhomBars;


        }

        /***************************************/

        private List<Node> ReadNodes(List<string> ids = null)
        {
            //Implement code for reading nodes
            throw new NotImplementedException();
        }

        /***************************************/

        private List<ISectionProperty> ReadSectionProperties(List<string> ids = null)
        {



            throw new NotImplementedException();
        }

        /***************************************/

        private List<Material> ReadMaterials(List<string> ids = null)
        {
            //Implement code for reading materials
            throw new NotImplementedException();
        }

        /***************************************************/


    }
}
