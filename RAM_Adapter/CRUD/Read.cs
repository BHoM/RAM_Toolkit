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
            
            
            // ***Should return list of "Bar", just testing with strings

            //Implement code for reading section properties
            

            List<string> ColumnSections = new List<string>();
            List<Bar> bhomBars = new List<Bar>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            // Get stories
            IStories IStories = IModel.GetStories();
            int numStories = IStories.GetCount();

            // Get columns on first story

            for (int i = 0; i < numStories; i++)
            {
                IColumns IColumns = IStories.GetAt(i).GetColumns();
                int numColumns = IColumns.GetCount();

                // Find name of every column (to begin)
                for (int j = 0; j < IColumns.GetCount(); j++)
                {

                    // Get the name of every column
                    IColumn IColumn = IColumns.GetAt(i);
                    string section = IColumn.strSectionLabel;
                    ColumnSections.Add(section);
                }

            }

            for (int i = 0; i < ColumnSections.Count(); i++)
            {
                Node startNode = null; 
                Node endNode = null; 
                Bar bhomBar = new Bar { StartNode = startNode, EndNode = endNode, Name = ColumnSections[i] };

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
