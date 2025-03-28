/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
        /**** Private method                            ****/
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
                int ID = (int)GetAdapterId(bar);

                if (BH.Engine.Structure.Query.IsVertical(bar))
                {
                    IColumn IColumn = IModel.GetColumn(ID);
                    ILayoutColumn ILayoutColumn = IModel.GetLayoutColumn(ID);

                    // Move column
                    double xStart = bar.Start.Position.X;
                    double yStart = bar.Start.Position.Y;
                    double zStart = bar.Start.Position.Z;
                    double xEnd = bar.End.Position.X;
                    double yEnd = bar.End.Position.Y;
                    double zEnd = bar.End.Position.Z;

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






