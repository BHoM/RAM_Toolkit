/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.SettingOut;
using BH.Engine.Units;
using BH.Engine.Adapter;
using BH.Engine.Base;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private List<NodeReaction> ReadNodeReaction(List<string> ids = null)
        {

            //Implement code for reading Node Reactions
            List<NodeReaction> bhomNodeReactions = new List<NodeReaction>();

            IModel ramModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            ILoadCases ramLoadCases = ramModel.GetLoadCases(EAnalysisResultType.RAMFrameResultType);
            //Get IWalls
            List<IWall> allRamWalls = ReadRamWalls(ramModel);

            // Adding node reactions per wall per loadcase, this is node reactions at btm of wall
            foreach (IWall wall in allRamWalls)
            {

                for (int i = 0; i < ramLoadCases.GetCount(); i++)
                {
                    //Get Loadcases
                    ILoadCase ramLoadCase = ramLoadCases.GetAt(i);
                    IPointLoads wallNodeForces = wall.GetNodeForcesAtEdge(EAnalysisResultType.RAMGravityResultType, ramLoadCase.lUID, EEdge.eBottomEdge);
                    for (int j = 0; j < wallNodeForces.GetCount(); j++)
                    {
                        //Get Node Forces
                        IPointLoad wallNodeForce = wallNodeForces.GetAt(j);
                        NodeReaction bhomNodeReaction = wallNodeForce.ToBHoMObject(ramLoadCase);
                        bhomNodeReactions.Add(bhomNodeReaction);
                    }
                }
            }

            return bhomNodeReactions;
        }

        /***************************************************/

        private List<BarDeformation> ReadBarDeformations(List<string> ids = null)
        {
            List<BarDeformation> bhomBarDeformations = new List<BarDeformation>();
            List<IBeam> ramBeams = ReadRamBeams(m_Model);
            foreach (IBeam ramBeam in ramBeams)
            {
                int beamID = ramBeam.lUID;
                //TODO: read deflections to go here            
            }
            return bhomBarDeformations;
        }

        /***************************************************/

        private List<RAMFactoredEndReactions> ReadBeamEndReactions(List<string> ids = null)
        {
            List<RAMFactoredEndReactions> barEndReactions = new List<RAMFactoredEndReactions>();

            IModel ramModel = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            List<IBeam> ramBeams = ReadRamBeams(ramModel);

            foreach (IBeam beam in ramBeams)
            {
                IAnalyticalResult result = beam.GetAnalyticalResult();
                IMemberForces forces = result.GetMaximumComboReactions(COMBO_MATERIAL_TYPE.GRAV_STEEL);

                RAMFactoredEndReactions bhomEndReactions = new RAMFactoredEndReactions()
                {
                    ObjectId = beam.lUID,
                    StartReaction = forces.GetAt(0).ToBHoMObject(),
                    EndReaction = forces.GetAt(1).ToBHoMObject(),
                };

                barEndReactions.Add(bhomEndReactions);
            }

            return barEndReactions;
        }

        /***************************************************/

    }

}



