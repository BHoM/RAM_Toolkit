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

            if (objects.First() is Bar)
            {
                success = CreateCollection(objects as IEnumerable<Bar>);
            }

            //// Commented out to just read Bar for Testing
            //success = CreateCollection(objects as dynamic);

            //UpdateViews()             //If there exists a command for updating the views is the software call it now:

            return success;             //Finally return if the creation was successful or not

        }

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<Bar> bars)
        {
            //Code for creating a collection of bars in the software
         
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
            
            //Access model
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Create floor type at each level
            IFloorTypes IFloorTypes = IModel.GetFloorTypes();

            for (int i = 0; i < levelHeights.Count(); i++)
            {
                IFloorTypes.Add("Level_" + levelHeights[i].ToString());
                IStories IStories = IModel.GetStories();
                IStories.Add(i, "Level " + i.ToString(), levelHeights[i]);
            }

            // Cycle through floortypes, access appropriate story, place beams on those stories
            for (int i = 0; i < IFloorTypes.GetCount(); i++)
            {
                IFloorType IFloorType = IFloorTypes.GetAt(i);
                ILayoutBeams ILayoutBeams = IFloorType.GetLayoutBeams();
                ILayoutColumns ILayoutColumns = IFloorType.GetLayoutColumns();

                //Cycle through bars; if z of bar = the floor height, add it
                for (int j = 0; j < beams.Count(); j++)
                {
                    //If bar is on level, add it during that iteration of the loop 
                    Bar bar = beams[j];

                    double xStart = bar.StartNode.Position.X;
                    double yStart = bar.StartNode.Position.Y;
                    double xEnd = bar.EndNode.Position.X;
                    double yEnd = bar.EndNode.Position.Y;

                    if (bar.StartNode.Position.Z == levelHeights[i])
                    {
                        ILayoutBeam ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, xStart, yStart, 0, xEnd, yEnd, 0);
                        ILayoutBeam.strSectionLabel = bar.SectionProperty.Name;
                    }
                }

                //Cycle through columns; if z of column = the floor height, add it
                for (int j = 0; j < columns.Count(); j++)
                {
                    //If bar is on level, add it during that iteration of the loop 
                    Bar bar = columns[j];

                    double xStart = bar.StartNode.Position.X;
                    double yStart = bar.StartNode.Position.Y;
                    double zStart = bar.StartNode.Position.Z;
                    double xEnd = bar.EndNode.Position.X;
                    double yEnd = bar.EndNode.Position.Y;
                    double zEnd = bar.EndNode.Position.Z;


                    if (bar.StartNode.Position.Z == levelHeights[i])
                    {
                        if (zStart <= zEnd)
                        {
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.ESteelMat, xStart, yStart, 0, 0);
                            ILayoutColumn.strSectionLabel = bar.SectionProperty.Name;
                        } else
                        {
                            ILayoutColumn ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.ESteelMat, xEnd, yEnd, 0, 0);
                            ILayoutColumn.strSectionLabel = bar.SectionProperty.Name;
                        }
                    }
                }
            }

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

            foreach (ISectionProperty sectionProperty in sectionProperties)
            {
                //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
                object secPropId = sectionProperty.CustomData[AdapterId];
                //If also the default implmentation for the DependencyTypes is used,
                //one can from here get the id's of the subobjects by calling (cast into applicable type used by the software): 
                object materialId = sectionProperty.Material.CustomData[AdapterId];
            }

            throw new NotImplementedException();
        }

        /***************************************************/

        private bool CreateCollection(IEnumerable<Material> materials)
        {
            //Code for creating a collection of materials in the software

            foreach (Material material in materials)
            {
                //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
                object materialId = material.CustomData[AdapterId];
            }

            throw new NotImplementedException();
        }


        /***************************************************/
    }
}
