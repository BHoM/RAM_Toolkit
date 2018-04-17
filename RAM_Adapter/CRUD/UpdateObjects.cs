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

        //Method being called for any object already existing in the model in terms of comparers is found.
        //Default implementation first deletes these objects, then creates new ones, if not applicable for the software, override this method

        protected override bool UpdateObjects<T>(IEnumerable<T> objects)
        {
            bool success = true;
            success = Update(objects as dynamic);
            return success;
        }

        protected bool Update(IEnumerable<IBHoMObject> bhomObjects)
        {
            return true;
        }

        // Essentially the same as the create method; experimenting with when it gets called by the BHoM "push" component
        protected bool Update(IEnumerable<Bar> bars)
        {

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

            }



            return true;
        }

        /***************************************************/
    }
}
