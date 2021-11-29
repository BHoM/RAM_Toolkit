/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.RAM;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Results;
using BH.oM.Structure.Loads;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.Requests;
using BH.oM.Analytical.Results;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.Engine.Base;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> IRead(Type type, IList ids, ActionConfig actionConfig = null)
        {
            dynamic elems = null;
            //Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            if (type == typeof(Bar))
                elems = ReadBars(ids as dynamic);
            else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
                elems = ReadSectionProperties(ids as dynamic);
            else if (type == typeof(IMaterialFragment))
                elems = ReadMaterials(ids as dynamic);
            else if (type == typeof(Panel))
                elems = ReadPanels(ids as dynamic);
            else if (type == typeof(ISurfaceProperty))
                elems = ReadISurfaceProperties(ids as dynamic);
            else if (type == typeof(LoadCombination))
                elems = ReadLoadCombination(ids as dynamic);
            else if (type == typeof(Loadcase))
                elems = ReadLoadCase(ids as dynamic);
            else if (type == typeof(Level))
                elems = ReadLevel(ids as dynamic);
            else if (type == typeof(Grid))
                elems = ReadGrid(ids as dynamic);
            else if (type == typeof(RAMPointGravityLoad))
                elems = ReadPointGravityLoad(ids as dynamic);
            else if (type == typeof(RAMLineGravityLoad))
                elems = ReadLineGravityLoad(ids as dynamic);
            else if (type == typeof(RAMFactoredEndReactions))
                elems = ReadBeamEndReactions(ids as dynamic);
            else if (type == typeof(ContourLoadSet))
                elems = ReadContourLoadSets(ids as dynamic);
            else if (type == typeof(UniformLoadSet))
                elems = ReadUniformLoadSets(ids as dynamic);
            if (typeof(IResult).IsAssignableFrom(type))
            {
                Modules.Structure.ErrorMessages.ReadResultsError(type);
                return null;
            }

            return elems;
        }

        /***************************************************/
    }

}


