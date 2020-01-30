/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Loads;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Structure;
using BH.oM.Adapter;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        //Method being called for any object already existing in the model in terms of comparers is found.
        //Default implementation first deletes these objects, then creates new ones, if not applicable for the software, override this method

        protected override bool IUpdate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            bool success = true;
            success = Update(objects as dynamic);
            return success;
        }

        /***************************************************/

        protected bool Update(IEnumerable<IBHoMObject> bhomObjects)
        {
            return true;
        }

        /***************************************************/

        // Essentially the same as the create method; experimenting with when it gets called by the BHoM "push" component
        protected bool Update(IEnumerable<Bar> bars)
        {

            //Access model
            IDBIO1 RAMDataAccIDBIO = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            IModel IModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            foreach (Bar bar in bars)
            {
                //Extract ID
                int ID = System.Convert.ToInt32(bar.CustomData[AdapterIdName]);

                if (BH.Engine.Structure.Query.IsVertical(bar))
                {
                    IColumn IColumn = IModel.GetColumn(ID);
                    ILayoutColumn ILayoutColumn = IModel.GetLayoutColumn(ID);

                    // Move column
                    double xStart = bar.StartNode.Position().X;
                    double yStart = bar.StartNode.Position().Y;
                    double zStart = bar.StartNode.Position().Z;
                    double xEnd = bar.EndNode.Position().X;
                    double yEnd = bar.EndNode.Position().Y;
                    double zEnd = bar.EndNode.Position().Z;

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

