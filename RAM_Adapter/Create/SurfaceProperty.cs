/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Constraints;
using BH.oM.Structure.MaterialFragments;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using BH.Engine.Base;
using BH.Engine.Structure;
using BH.Engine.Spatial;
using BH.Adapter.RAM;
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Structure.Loads;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<ISurfaceProperty> srfProps)
        {
            foreach (ISurfaceProperty srfProp in srfProps)
            {
                RAMId RAMId = new RAMId();
                if (srfProp is Ribbed)
                {
                    Ribbed compProp = (Ribbed)srfProp;
                    ICompDeckProp ramProp;

                    //Validity check for ribbed properties thickness above flutes
                    if (compProp.Thickness < 2.0.FromInch())
                    {
                        Engine.Base.Compute.RecordError("Deck property " + srfProp.Name + " has an invalid thickness. Thickness was automatically changed to 2 inches in order to ensure required shear stud engagement per RAM.");
                        compProp.Thickness = 2.0.FromInch();
                    }
                    ICompDeckProps compDeckProps = m_Model.GetCompositeDeckProps();
                    try
                    {
                        ramProp = compDeckProps.Add2(compProp.Name, Engine.Base.Query.PropertyValue(compProp, "DeckProfileName").ToString(), compProp.Thickness.ToInch(), compProp.TotalDepth.ToInch() - 1.5);
                    }
                    catch
                    {
                        Engine.Base.Compute.RecordWarning("Deck label for surface property " + srfProp.Name + " not found or invalid for specified thickness. Using default deck profile. Please provide a valid deck name from the RAM Deck Table as a property on the SurfaceProperty named DeckProfileName.");
                        ramProp = compDeckProps.Add2(compProp.Name, "ASC 3W", compProp.Thickness.ToInch(), 4.5);
                    }
                    RAMId.Id = ramProp.lUID;
                }
                else if (srfProp is ConstantThickness && !(srfProp.Material is Steel))
                {
                    ConstantThickness prop = (ConstantThickness)srfProp;
                    if (prop.PanelType != PanelType.Wall)  //Wall surface properties are created on a per wall element basis
                    {
                        IConcSlabProps concSlabProps = m_Model.GetConcreteSlabProps();
                        IConcSlabProp ramProp;
                        ramProp = concSlabProps.Add(prop.Name, prop.Thickness.ToInch(), prop.Material.Density.ToPoundPerCubicFoot());
                        RAMId.Id = ramProp.lUID;
                    }
                }
                else if (srfProp is ConstantThickness && srfProp.Material is Steel)
                {
                    ConstantThickness prop = (ConstantThickness)srfProp;
                    if (prop.PanelType != PanelType.Wall)  //Wall surface properties are created on a per wall element basis
                    {
                        INonCompDeckProps nonCompDeckProps = m_Model.GetNonCompDeckProps();
                        INonCompDeckProp ramProp;

                        ramProp = nonCompDeckProps.Add(prop.Name);
                        ramProp.dEffectiveThickness = prop.Thickness.ToInch();
                        RAMId.Id = ramProp.lUID;
                    }
                }
                srfProp.SetAdapterId(RAMId);
            }

            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }

        /***************************************************/
    }
}


