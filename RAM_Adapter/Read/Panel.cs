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
        private List<ISurfaceProperty> ReadISurfaceProperties(List<string> ids = null)
        {

            List<ISurfaceProperty> propList = new List<ISurfaceProperty>();
            ISteelCriteria steelCriteria = m_Model.GetSteelCriteria();
            IDeckTableEntries deckProfiles = steelCriteria.GetDeckTableEntries();

            ICompDeckProps compDeckProps = m_Model.GetCompositeDeckProps();
            for (int i = 0; i < compDeckProps.GetCount(); i++)
            {
                ICompDeckProp DeckProp = compDeckProps.GetAt(i);
                string deckLabel = DeckProp.strLabel;
                string deckProfileName = DeckProp.strDeckType;
                IDeckTableEntry profile = null;

                for (int j = 0; j < deckProfiles.GetCount(); j++) // find ram deck profile to get props
                {
                    profile = deckProfiles.GetAt(j);
                    if (profile.strDeckName == deckLabel)
                    { break; }
                }

                double concThickness = DeckProp.dThickAboveFlutes.FromInch();
                double deckProfileThickness = profile.dTD.FromInch();
                double deckThickness = concThickness + deckProfileThickness;

                IMaterialFragment material = Engine.Structure.Create.Concrete("Concrete Over Deck");

                Ribbed deck2DProp = new Ribbed();
                deck2DProp.Name = deckLabel;
                deck2DProp.Thickness = concThickness;
                deck2DProp.PanelType = PanelType.Slab;
                deck2DProp.Material = material;
                deck2DProp.Spacing = profile.dRSpac;
                deck2DProp.StemWidth = profile.dWR;
                deck2DProp.TotalDepth = deckThickness;

                // Unique RAM ID
                RAMId RAMId = new RAMId();
                RAMId.Id = DeckProp.lUID;
                deck2DProp.SetAdapterId(RAMId);

                RAMDeckData ramDeckData = new RAMDeckData();
                ramDeckData.DeckProfileName = deckProfileName;
                deck2DProp.Fragments.Add(ramDeckData);

                propList.Add(deck2DProp);
            }

            IConcSlabProps concSlabProps = m_Model.GetConcreteSlabProps();
            for (int i = 0; i < concSlabProps.GetCount(); i++)
            {
                IConcSlabProp DeckProp = concSlabProps.GetAt(i);
                double deckThickness = DeckProp.dThickness.FromInch();
                string deckLabel = DeckProp.strLabel;
                IMaterialFragment material = Engine.Structure.Create.Concrete("Concrete");

                ConstantThickness deck2DProp = new ConstantThickness();
                deck2DProp.Name = deckLabel;
                deck2DProp.Material = material;
                deck2DProp.Thickness = deckThickness;
                deck2DProp.PanelType = PanelType.Slab;
                propList.Add(deck2DProp);

                // Unique RAM ID
                RAMId RAMId = new RAMId();
                RAMId.Id = DeckProp.lUID;
                deck2DProp.SetAdapterId(RAMId);
            }

            INonCompDeckProps nonCompDeckProps = m_Model.GetNonCompDeckProps();
            for (int i = 0; i < nonCompDeckProps.GetCount(); i++)
            {
                INonCompDeckProp DeckProp = nonCompDeckProps.GetAt(i);
                double deckThickness = DeckProp.dEffectiveThickness.FromInch();
                string deckLabel = DeckProp.strLabel;
                IMaterialFragment material = Engine.Structure.Create.Steel("Metal Deck");

                ConstantThickness deck2DProp = new ConstantThickness();
                deck2DProp.Name = deckLabel;
                deck2DProp.Material = material;
                deck2DProp.Thickness = deckThickness;
                deck2DProp.PanelType = PanelType.Slab;
                propList.Add(deck2DProp);

                // Unique RAM ID
                RAMId RAMId = new RAMId();
                RAMId.Id = DeckProp.lUID;
                deck2DProp.SetAdapterId(RAMId);
            }

            return propList;
        }

        /***************************************************/

        private List<Panel> ReadPanels(List<string> ids = null)
        {
            //Get dictionary of surface properties with ids
            Dictionary<string, ISurfaceProperty> bhomProperties = ReadISurfaceProperties().ToDictionary(x => GetAdapterId(x).ToString());

            //Implement code for reading panels
            List<Panel> bhomPanels = new List<Panel>();

            //Get stories
            IStories IStories = m_Model.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {

                //Get Walls
                IWalls IWalls = IStories.GetAt(i).GetWalls();
                int numWalls = IWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall IWall = IWalls.GetAt(j);
                    Panel Panel = BH.Adapter.RAM.Convert.ToBHoMObject(IWall);
                    bhomPanels.Add(Panel);
                }

                //Get Floors
                IStory IStory = IStories.GetAt(i);
                IFloorType IFloorType = IStory.GetFloorType();
                IDecks IDecks = IFloorType.GetDecks();
                int IStoryUID = IStory.lUID;

                int numDecks = IDecks.GetCount();

                // Convert Floors
                for (int j = 0; j < numDecks; j++)
                {
                    IDeck IDeck = IDecks.GetAt(j);
                    try
                    {
                        Panel panel = BH.Adapter.RAM.Convert.ToBHoMObject(IDeck, m_Model, IStoryUID);
                        panel.Property = bhomProperties[IDeck.lPropID.ToString()];
                        bhomPanels.Add(panel);
                    }
                    catch
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("This story has no slab edges defined. IStoryUID: " + IStoryUID);
                    }
                }
            }

            return bhomPanels;
        }

        /***************************************************/

        private List<IWall> ReadRamWalls(IModel ramModel)
        {
            //Get stories
            IStories ramStories = ramModel.GetStories();
            int numStories = ramStories.GetCount();
            List<IWall> allRamWalls = new List<IWall>();

            // Get all walls on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Walls
                IWalls ramWalls = ramStories.GetAt(i).GetWalls();
                int numWalls = ramWalls.GetCount();

                // Convert Walls
                for (int j = 0; j < numWalls; j++)
                {
                    IWall ramWall = ramWalls.GetAt(j);
                    allRamWalls.Add(ramWall);
                }
            }

            return allRamWalls;
        }

        /***************************************************/
    }
}