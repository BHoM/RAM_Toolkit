/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using RAMDATAACCESSLib;
using RAMDataBaseAccess;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.oM.Geometry;

namespace RAM_Test
{

    public class Program
    {

        static void Main(string[] args)
        {

            Test();

        }

        private static void Test()

        {

            string filePathNew;
            string filePathExisting;
            string filePathUserfile;
            string filePathAdd;
            string strWorkingDir;
            string filePathEdited;
            Boolean run;
            int type;
            List<int> Stories;
            Boolean loaded;
            IStories IStories;

            // Define Variables
            IRamDataAccess1 RAMDataAcc1;
            IDBIO1 RAMDataAccIDBIO;
            IModel IModel;
            ISteelCriteria ISteelCriteria;
            IModelData1 IModelData1;
            IStory IStory;
            IBeams IBeams;
            IColumns IColumns;
            IFloorTypes IFloorTypes;
            IFloorType IFloorType;
            ILayoutColumns ILayoutColumns;
            ILayoutBeams ILayoutBeams;
            IGridSystem IGridSystem; 

            Stories = new List<int>();
            List<string> ColumnSections = new List<string>();
            List<string> ColumnStartX = new List<string>();

            // Set filepaths (New can be any filepath, existing has to be an actual model; will give errors if interface has not been released, still working on it)
            filePathNew = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\testCreate.rss";
            filePathExisting = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\Tutorial_v1507_US.rss";
            filePathAdd = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\testAdd.rss";
            strWorkingDir = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial";
            filePathEdited = filePathExisting.Replace(".rss", "API.rss");

            // Usr filepath so we can delete .usr at end of function
            filePathUserfile = filePathExisting.Replace(".rss", ".usr");

            // Initialize Data Access
            RAMDataAcc1 = new RamDataAccess1();

           

            // Set Type (for testing)
            string Type = "Create";
            //string Type = "Add";
            //string Type = "Existing";

            


            //////////////////////////////////////////////////////////////////////////////

            filePathExisting = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\Tutorial_v1507_US.rss";
            string filePathExisting2 = "O:\\0040157 Minneapolis Civic Center\\F04 Structures\\04 Calculations\\DD\\07_Gravity Design\\Local Models -By Floor\\10_Level 10\\0040157_20180201_MCOB Steel framing LVL 10 Penthouse Roof.rss";

            RAMDataAccIDBIO = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
            
            
            //RAMDataAccIDBIO.LoadDataBase(filePathExisting);
           

            //// Object Model Interface

            //IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            
            //// Get stories
            //IStories = IModel.GetStories();
            //int numStories = IStories.GetCount();


            ////Testing getting NODES

            //INodes INodes = IModel.GetFrameAnalysisNodes();
            //int numNodes = INodes.GetCount();
            //List<INode> INodesList = new List<INode>();

            //for (int i = 0; i < numNodes; i++)
            //{
            //    //Get Nodes
            //    INode INode = INodes.GetAt(i);
            //    INodesList.Add(INode);

            //    // Get the location of the node
            //    SCoordinate Location = new SCoordinate();
            //    Location = INode.sLocation;

            //    BH.oM.Structure.Elements.Node Node = new Node();

            //    Node.Position = new BH.oM.Geometry.Point() { X = Location.dXLoc, Y = Location.dYLoc, Z = Location.dZLoc };
            //    IDisplacements IDisplacements = INode.GetDisplacements();
            //   // IMemberForces IMemberForces = INode.GetReactions();




            //    for (int j = 0; j < IDisplacements.GetCount(); j++)
            //    {
            //        IDisplacement IDisplacement = IDisplacements.GetAt(j);

            //        double x = IDisplacement.dDispX;
            //        double y = IDisplacement.dDispY;
            //        double z = IDisplacement.dDispZ;
            //        double thetax = IDisplacement.dThetaX;
            //        double thetay = IDisplacement.dThetaY;
            //        double thetaz = IDisplacement.dThetaZ;

            //        Node.CustomData["dX"] = x;
            //        Node.CustomData["dY"] = y;
            //        Node.CustomData["dZ"] = z;
            //        Node.CustomData["dthetaX"] = thetax;
            //        Node.CustomData["dthetaY"] = thetay;
            //        Node.CustomData["dthetaZ"] = thetaz;

            //    }

            //for (int j = 0; j < IMemberForces.GetCount(); j++)
            //{
            //    IMemberForce IMemberForce = IMemberForces.GetAt(j);
            //    IMemberForce.ToString();
            //    Node.CustomData["Reaction" + i.ToString()] = IMemberForce.ToString();
            //    Console.Write(IMemberForce.ToString());
            //}

        
            //int test1 = 1;


            ///////////////////////////////////////////////////////////////////////////////





            // Initialize to interface (CREATE NEW MODEL)
            if (Type.Equals("Create"))
            {

                RAMDataAccIDBIO.CreateNewDatabase2(filePathNew, EUnits.eUnitsEnglish, "Grasshopper");

                // Object Model Interface
                IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

                // Testing element creation

                IFloorTypes = IModel.GetFloorTypes();
                IFloorTypes.Add("Level 1");
                IFloorType = IFloorTypes.GetAt(0);
                ILayoutColumns = IFloorType.GetLayoutColumns();
                ILayoutBeams = IFloorType.GetLayoutBeams();

                //Assign floor types to appropriate stories
                IStories = IModel.GetStories();
                IStories.Add(IFloorType.lUID, "Level 1", 120);

                // Once we have the ILayoutColumn we can do iterative creation with list of input points, properties, etc
                // NOTE THAT RAM INTERNAL API UNITS ARE INCHES, EVEN WHEN DISPLAY UNITS ARE FEET

                //Column parameters are, essentially, floor type, material, x loc, y loc, z offset start, z offset end.
                //From BHoM, this will be translated as a story that calls an appropriate Floor type interface which then calls the appropriate parameters.

                ILayoutColumn ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.ESteelMat, 0, 0, 0, 0);
                ILayoutColumn.strSectionLabel = "W14X48";
                ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.EConcreteMat, 240, 0, 0, 0);
                ILayoutColumn.strSectionLabel = "C12X26";
                ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.EConcreteMat, 0, 240, 0, 0);
                ILayoutColumn.strSectionLabel = "C12X26";
                ILayoutColumn = ILayoutColumns.Add(EMATERIALTYPES.EConcreteMat, 240, 240, 0, 0);
                ILayoutColumn.strSectionLabel = "C12X26";

                //Beam parameters are similar to column parameters. It is accordingly logical to use the adapter to build out each
                //floor type with these geometries and then apply the floor types to stories according to the input model

                ILayoutBeam ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 0, 0, 240, 0, 0);
                ILayoutBeam.strSectionLabel = "W14X48";
                ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 240, 0, 240, 240, 0);
                ILayoutBeam.strSectionLabel = "W14X48";
                ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 240, 0, 0, 240, 240, 0);
                ILayoutBeam.strSectionLabel = "W14X48";
                ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 0, 0, 0, 240, 0);
                ILayoutBeam.strSectionLabel = "W14X48";

                filePathUserfile = filePathNew.Replace(".rss", ".usr");

            }







            //    // Existing model, initialize model and then add columns
            //    if (Type.Equals("Add"))
            //    {

            //        filePathUserfile = filePathAdd.Replace(".rss", ".usr");
            //        RAMDataAccIDBIO.LoadDataBase(filePathAdd);

            //        // Object Model Interface
            //        IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //        // Access floor type interface

            //        IFloorTypes = IModel.GetFloorTypes();
            //        IFloorType = IFloorTypes.GetAt(0);
            //        ILayoutColumns = IFloorType.GetLayoutColumns();
            //        ILayoutBeams = IFloorType.GetLayoutBeams();

            //        // Create columns

            //        ILayoutColumn ILayoutColumn = ILayoutColumns.Add2(EMATERIALTYPES.ESteelMat, 0, 0, 0, 0, 0, 0);
            //        ILayoutColumn.strSectionLabel = "W14X48";
            //        ILayoutColumn = ILayoutColumns.Add2(EMATERIALTYPES.EConcreteMat, 240, 0, 0, 0, 0, 0);
            //        ILayoutColumn.strSectionLabel = "C12X26";
            //        ILayoutColumn = ILayoutColumns.Add2(EMATERIALTYPES.EConcreteMat, 0, 240, 0, 0, 0, 0);
            //        ILayoutColumn.strSectionLabel = "C12X26";
            //        ILayoutColumn = ILayoutColumns.Add2(EMATERIALTYPES.EConcreteMat, 240, 240, 0, 0, 0, 0);
            //        ILayoutColumn.strSectionLabel = "C12X26";

            //        //Create beams

            //        ILayoutBeam ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 0, 0, 240, 0, 0);
            //        ILayoutBeam.strSectionLabel = "W14X48";
            //        ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 240, 0, 240, 240, 0);
            //        ILayoutBeam.strSectionLabel = "W14X48";
            //        ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 240, 0, 0, 240, 240, 0);
            //        ILayoutBeam.strSectionLabel = "W14X48";
            //        ILayoutBeam = ILayoutBeams.Add(EMATERIALTYPES.ESteelMat, 0, 0, 0, 0, 240, 0);
            //        ILayoutBeam.strSectionLabel = "W14X48";

            //        filePathUserfile = filePathAdd.Replace(".rss", ".usr");
            //    }






            //        // Initialize to interface (FOR EXISTING MODEL)
            //        if (Type.Equals("Existing")) {

            //        filePathUserfile = filePathExisting.Replace(".rss", ".usr");
            //        RAMDataAccIDBIO.LoadDataBase(filePathExisting);

            //        // Object Model Interface
            //        IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //        // Get stories
            //        IStories = IModel.GetStories();
            //        int numStories = IStories.GetCount();
            //        Stories.Add(numStories);



            //        // Get columns on first story
            //        IColumns = IStories.GetAt(1).GetColumns();
            //        int numColumns = IColumns.GetCount();

            //        // Find name of every column (to begin)
            //        for (int i = 0; i < IColumns.GetCount(); i++)
            //        {

            //            // Get the name of every column
            //            IColumn IColumn = IColumns.GetAt(i);
            //            string section = IColumn.strSectionLabel;
            //            ColumnSections.Add(section);

            //            SCoordinate startPt = new SCoordinate();
            //            SCoordinate endPt = new SCoordinate();
            //            IColumn.GetEndCoordinates(ref startPt, ref endPt);
            //            double x = startPt.dXLoc;
            //            string xStr = x.ToString();
            //            ColumnSections.Add(xStr);

            //        }

            //        //Write output of original database
            //        Console.WriteLine(filePathExisting);
            //        Stories.ForEach(i => Console.Write("{0}\t", i));
            //        ColumnSections.ForEach(i => Console.Write("{0}\t", i));

            //        // Set every column to a standard section size
            //        //for (int i = 0; i < IColumns.GetCount(); i++)
            //        //{

            //        //     Set every column to a standard size (working, need to save database after update)
            //        //    IColumn IColumn = IColumns.GetAt(i);
            //        //    IColumn.strSectionLabel = "W14X48";

            //        //}

            //        // Find name of every column (to check updated section names)
            //        ColumnSections.Clear();
            //        for (int i = 0; i < IColumns.GetCount(); i++)
            //        {

            //            // Get the name of every column
            //            IColumn IColumn = IColumns.GetAt(i);
            //            string section = IColumn.strSectionLabel;
            //            ColumnSections.Add(section);

            //        }

            //    }

            //    //Write output of new database
            //    Console.WriteLine(filePathExisting);
            //    Stories.ForEach(i => Console.Write("{0}\t", i));
            //    ColumnSections.ForEach(i => Console.Write("{0}\t", i));

            //    //Save file
            //    RAMDataAccIDBIO.SaveDatabase();

            //    // Release main interface and delete user file
            //    RAMDataAccIDBIO = null;
            //    System.IO.File.Delete(filePathUserfile);

            //    int test = 1;
            //}

        }




    }
}

