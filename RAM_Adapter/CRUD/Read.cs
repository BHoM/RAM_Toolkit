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

            //Implement code for reading section properties
            
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
