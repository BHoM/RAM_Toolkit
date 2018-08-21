using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Properties;
using BH.oM.Structure.Loads;
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

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            foreach (Bar bar in bars)
            {
                //Extract ID
                int ID = System.Convert.ToInt32(bar.CustomData["lUID"]);

                if (BH.Engine.Structure.Query.IsVertical(bar))
                {
                    IColumn IColumn = IModel.GetColumn(ID);
                    ILayoutColumn ILayoutColumn = IModel.GetLayoutColumn(ID);

                    // Move column
                    double xStart = bar.StartNode.Position.X;
                    double yStart = bar.StartNode.Position.Y;
                    double zStart = bar.StartNode.Position.Z;
                    double xEnd = bar.EndNode.Position.X;
                    double yEnd = bar.EndNode.Position.Y;
                    double zEnd = bar.EndNode.Position.Z;

                    // Need level rather than offset for setting (still in progress)
                    ILayoutColumn.SetLayoutCoordinates2(xStart, yStart, zStart, xEnd, yEnd, zEnd);
                    
                    // Change section property of column
                    IColumn.strSectionLabel = bar.Name;

                }
                else {
                    IBeam IBeam = IModel.GetBeam(ID);
                    ILayoutBeam ILayoutBeam = IModel.GetLayoutBeam(ID);

                    // Change section property of column
                    IBeam.strSectionLabel = bar.Name;

                }
                       

            }


            return true;
        }

        /***************************************************/
    }
}
