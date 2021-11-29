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
using BH.oM.Geometry.SettingOut;
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

        private bool CreateCollection(IEnumerable<Grid> bhomGrid)
        {
            // Register GridSystems
            IGridSystems ramGridSystems = m_Model.GetGridSystems();

            // Register FloorTypes
            IFloorTypes ramFloorTypes = m_Model.GetFloorTypes();

            // initializa a BhoM grid
            List<Grid> Grids = bhomGrid.ToList();

            //Split grids by gridtypes
            List<Grid> XGrids = new List<Grid>();
            List<Grid> YGrids = new List<Grid>();
            List<Grid> skewGrids = new List<Grid>();
            List<Grid> circGrids = new List<Grid>();
            Grid grid = new Grid();
            Polyline gridLine = new Polyline();

            //create different names for the gridSystem based on if there are items in the list
            double gridSystemRotation = 0;
            string gridSystemLabel = "";
            IGridSystem ramGridSystemXY = null;
            //IGridSystem ramGridSystemRad = null;
            //IGridSystem ramGridSystemSk = null;
            IModelGrids ramModelGridsXY = null;
            //IModelGrids ramModelGridsRad = null;
            //IModelGrids ramModelGridsSk = null;


            //Loop through the BHoM grids and sort per type (x,y,radial, circular, skewed) 
            for (int i = 0; i < Grids.Count(); i++)
            {
                grid = Grids[i];

                if (grid.Curve is Circle)
                {
                    circGrids.Add(grid);
                }
                else
                {
                    gridLine = Engine.Geometry.Modify.CollapseToPolyline(grid.Curve as dynamic, 10);
                    //add lines to corresponding lists (XGrids, YGrids) based on their  orientation
                    if (Math.Abs(gridLine.StartPoint().X - gridLine.EndPoint().X) < 0.1)
                    {
                        YGrids.Add(grid);
                    }
                    else if (Math.Abs(gridLine.StartPoint().Y - gridLine.EndPoint().Y) < 0.1)
                    {
                        XGrids.Add(grid);
                    }
                    else
                    {
                        skewGrids.Add(grid);
                    }
                }
            }


            //Create grid systems per grid lists

            //XYGrids
            if (YGrids.Count() != 0 || XGrids.Count() != 0)
            {
                gridSystemLabel = "XY_grid";
                ramGridSystemXY = ramGridSystems.Add(gridSystemLabel);
                ramGridSystemXY.eOrientationType = SGridSysType.eGridOrthogonal;
                ramGridSystemXY.dRotation = gridSystemRotation;
                ramModelGridsXY = ramGridSystemXY.GetGrids();
            }


            // NOTE: Radial and Skewed Not Yet Implemented but code framework is below

            ////Radial Circular Grid
            //if (circGrids.Count() != 0)
            //{
            //    gridSystemLabel = "Radial_grid";
            //    ramGridSystemRad = ramGridSystems.Add(gridSystemLabel);
            //    ramGridSystemRad.dXOffset = gridOffsetX;
            //    ramGridSystemRad.dYOffset = gridOffsetY;
            //    ramGridSystemRad.eOrientationType = SGridSysType.eGridRadial;
            //    ramGridSystemRad.dRotation = gridSystemRotation;
            //    ramModelGridsRad = ramGridSystemRad.GetGrids();
            //}
            //// Skewed grid
            //if (skewGrids.Count() != 0) {
            //    gridSystemLabel = "Skew_gird";
            //    ramGridSystemSk = ramGridSystems.Add(gridSystemLabel);
            //    ramGridSystemSk.dXOffset = 0;
            //    ramGridSystemSk.dYOffset = 0;
            //    ramGridSystemSk.eOrientationType = SGridSysType.eGridSkewed;
            //    ramGridSystemSk.dRotation = gridSystemRotation;
            //    ramModelGridsSk = ramGridSystemSk.GetGrids();

            //}


            //  Get Grid System Offset
            double minY = XGrids[0].Curve.IStartPoint().Y.ToInch();
            double minX = YGrids[0].Curve.IStartPoint().X.ToInch();

            foreach (Grid XGrid in XGrids)
            {
                double gridY = XGrid.Curve.IStartPoint().Y.ToInch();
                if (gridY < minY)
                    minY = gridY;
            }

            foreach (Grid YGrid in YGrids)
            {
                double gridX = YGrid.Curve.IStartPoint().X.ToInch();
                if (gridX < minX)
                    minX = gridX;
            }
            ramGridSystemXY.dXOffset = minX;
            ramGridSystemXY.dYOffset = minY;


            // Create Grids in GridSystem
            foreach (Grid XGrid in XGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(XGrid.Curve as dynamic, 10);
                ramModelGridsXY.Add(XGrid.Name, EGridAxis.eGridYorCircularAxis, gridLine.StartPoint().Y.ToInch() - minY);
            }

            foreach (Grid YGrid in YGrids)
            {
                gridLine = Engine.Geometry.Modify.CollapseToPolyline(YGrid.Curve as dynamic, 10);
                ramModelGridsXY.Add(YGrid.Name, EGridAxis.eGridXorRadialAxis, gridLine.StartPoint().X.ToInch() - minX);
            }

            foreach (Grid cGrid in circGrids)
            {
                // TODO: add code to implement circular grids
                // Create GridSystem in RAM for each unique centerpt of circGrids  
            }

            foreach (Grid skGrid in skewGrids)
            {
                // TODO: add code to implement skewed grids
                // Create GridSystem in RAM for each unique angle of skewGrids
            }

            //get the ID of the gridsystem
            int gridSystemID = ramGridSystemXY.lUID;

            //Cycle through floortypes, access the existing floortype/story, place grids on those stories
            for (int i = 0; i < ramFloorTypes.GetCount(); i++)
            {
                IFloorType ramFloorType = ramFloorTypes.GetAt(i);
                DAArray gsID = ramFloorType.GetGridSystemIDArray();
                gsID.Add(ramGridSystemXY.lUID, 0);
                ramFloorType.SetGridSystemIDArray(gsID);
            }

            //Save file
            m_IDBIO.SaveDatabase();

            return true;
        }

        /***************************************************/
    }
}
