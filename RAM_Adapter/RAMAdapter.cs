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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.RAM;
using RAMDATAACCESSLib;
using System.IO;
using System.Net.NetworkInformation;

namespace BH.Adapter.RAM
{
    public partial class RAMAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
        public RAMAdapter(string filePath = "", bool Active = false)
        {

            if (Active)
            {
                AdapterId = BH.Engine.RAM.Convert.AdapterId;   //Set the "AdapterId" to "SoftwareName_id". Generally stored as a constant string in the convert class in the SoftwareName_Engine

                Config.SeparateProperties = true;   //Set to true to push dependant properties of objects before the main objects are being pushed. Example: push nodes before pushing bars
                Config.MergeWithComparer = true;    //Set to true to use EqualityComparers to merge objects. Example: merge nodes in the same location
                Config.ProcessInMemory = false;     //Set to false to update objects in the toolkit during the push
                Config.CloneBeforePush = true;      //Set to true to clone the objects before they are being pushed through the software. Required if any modifications at all, as adding a software ID is done to the objects
                Config.UseAdapterId = false;        //Tag objects with a software specific id in the CustomData. Requires the NextIndex method to be overridden and implemented


                m_RAMApplication = null;
                m_RAMApplication = new RamDataAccess1();
                IDBIO1 RAMDataAccIDBIO;
                IModel IModel;

                //CASE01 :  if NO filepath is proivded and NO .rss file exists 
                // Initialize to interface (CREATE NEW MODEL in RAM data folder by default)

                if (filePath == "" && !File.Exists(filePath))
                {
                    string filePathNew = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\BHoM_Model.rss";
                    try
                    {
                        RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                        // Object Model Interface
                        IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
                        RAMDataAccIDBIO.CreateNewDatabase2(filePathNew, EUnits.eUnitsEnglish, "Grasshopper");
                        // Delete usr file
                        File.Delete(filePathNew.Replace(".rss", ".usr"));

                    }
                    catch
                    {
                        Console.WriteLine("Cannot create RAM database, check that a compatible version of RAM is installed");
                    }

                }

                // if a file path is provided by the USER check it for Validity

                if (filePath != "") 
                {
                    //modify file path to ensure its validity
                    string filePathMod = filePath.Replace("\\\\", "\\");
                    filePathMod = filePathMod.Replace("\r\n", "");
                    filePath = filePathMod;
                    
                    //check if after modification file exists
                    //if the file does not exist create a new file
                    if (!File.Exists(filePath))
                    {
                        try
                        {
                            RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                            // Object Model Interface
                            IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
                            RAMDataAccIDBIO.CreateNewDatabase2(filePath, EUnits.eUnitsEnglish, "Grasshopper");
                            // Delete usr file
                            File.Delete(filePath.Replace(".rss", ".usr"));

                        }
                        catch
                        {
                            Console.WriteLine("Cannot create RAM database, check that a compatible version of RAM is installed");
                        }


                    }
                    else if (File.Exists(filePath))
                    {
                        // if an .rss file is provided
                        // Initialize to interface (OF EXISTING MODEL)

                        string fileName = Path.GetFileName(filePath);
                        string fileNameRAM = fileName.Replace(".rss", ".ram");
                        //Two possible working dirs depending on install
                        string filePathWorkingDir1 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Working\\";
                        string filePathWorkingDir2 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Working\\";
                        string filePathTempRAMFile1 = filePathWorkingDir1 + fileNameRAM;
                        string filePathTempRAMFile2 = filePathWorkingDir2 + fileNameRAM;
                        //Delete .ram file in working directory if it exists
                        if (File.Exists(filePathTempRAMFile1))
                        {
                            File.Delete(filePathTempRAMFile1);
                        }
                        if (File.Exists(filePathTempRAMFile2))
                        {
                            File.Delete(filePathTempRAMFile2);
                        }

                        RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                        double loadOutput = RAMDataAccIDBIO.LoadDataBase2(filePath, "Grasshopper"); //if 0 successful

                        //check if data base is properly loaded

                        if (loadOutput == 25673)
                        {
                            throw new ArgumentException("Cannot access RAM database. If file is open in RAM, please close. Otherwise, open the file in RAM, close RAM, and try again.");
                        }
                        else if (loadOutput == 25657)
                        {
                            File.Delete(filePath.Replace(".rss", ".usr"));       // Delete usr file
                            throw new ArgumentException("RAM Version installed does not match version of file.");
                        }
                        else if (loadOutput == 25674)
                        {
                            throw new ArgumentException(".rss and .ram file exist for same model.");
                        }
                        else if (loadOutput == 301)
                        {
                            throw new ArgumentException("Failed to read .ram file.");
                        }

                        // Object Model Interface
                        IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

                        // Delete usr file
                        System.IO.File.Delete(filePath.Replace(".rss", ".usr"));
                    }

                }//check of filepath ends here


            }

        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        //Add any comlink object as a private field here, example named:

        //private SoftwareComLink m_softwareNameCom;

        private RamDataAccess1 m_RAMApplication;


        /***************************************************/


    }
}
