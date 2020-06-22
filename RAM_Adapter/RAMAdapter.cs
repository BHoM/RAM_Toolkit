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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.Adapters.RAM;
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
        public RAMAdapter(string filePath = "", bool active = false)
        {

            if (active)
            {
                AdapterIdName = BH.Adapter.RAM.Convert.AdapterIdName;   //Set the "AdapterId" to "SoftwareName_id". Generally stored as a constant string in the convert class in the SoftwareName_Engine

                BH.Adapter.Modules.Structure.ModuleLoader.LoadModules(this);
                SetupDependencies();
                SetupComparers();

                m_Application = null;
                m_Application = new RamDataAccess1();
                m_IDBIO = null;

                //Validate Filepath
                if (filePath == "" || !File.Exists(filePath)) //No filepath given - set to default location
                {
                    Directory.CreateDirectory(filePath);

                    if (Path.GetFileName(filePath) == "")
                        filePath = Path.Combine(filePath, "BHoM_Model.rss");
                    m_filePath = @"C:\ProgramData\Bentley\Engineering\RAM Structural System\Data\BHoM_Model.rss"; ;
                    try
                    {
                        m_IDBIO = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                        // Create DB
                        m_IDBIO.CreateNewDatabase2(m_filePath, EUnits.eUnitsEnglish, "BHoM");
                        CloseDatabase();
                    }
                    catch
                    {
                        Engine.Reflection.Compute.RecordError("Cannot create RAM database, check that a compatible version of RAM is installed");
                    }
                }
                else if (File.Exists(filePath))
                {
                    // if an .rss file is provided
                    // Initialize to interface (OF EXISTING MODEL)

                    string fileNameRAM = Path.ChangeExtension(filePath, "ram");
                    string fileNameDbSdf = Path.ChangeExtension(filePath, "db.sdf");
                    //Two possible working dirs depending on install
                    string filePathWorkingDir1 = @"C:\ProgramData\Bentley\Engineering\RAM Structural System\Working\";
                    string filePathWorkingDir2 = @"C:\ProgramData\Bentley\Engineering\RAM Structural System\Data\Working\";
                    string filePathTempRAMFile1 = Path.Combine(filePathWorkingDir1, fileNameRAM);
                    string filePathTempRAMFile2 = Path.Combine(filePathWorkingDir2, fileNameRAM);
                    string filePathTempDBFile1 = Path.Combine(filePathWorkingDir1, fileNameDbSdf);
                    string filePathTempDBFile2 = Path.Combine(filePathWorkingDir2, fileNameDbSdf);

                    //Delete .ram file in working directory if it exists
                    if (File.Exists(filePathTempRAMFile1))
                    {
                        File.Delete(filePathTempRAMFile1);
                    }
                    if (File.Exists(filePathTempRAMFile2))
                    {
                        File.Delete(filePathTempRAMFile2);
                    }

                    //Check if temp db.sdf file is read-only
                    if (File.Exists(filePathTempDBFile1))
                    {
                        try { File.Delete(filePathTempDBFile1); }
                        catch { Engine.Reflection.Compute.RecordError("Working db.sdf file is in use. Please close BHOM UI and reopen."); }
                    }
                    if (File.Exists(filePathTempDBFile2))
                    {
                        try { File.Delete(filePathTempDBFile2); }
                        catch { Engine.Reflection.Compute.RecordError("Working db.sdf file is in use. Please close BHOM UI and reopen."); }
                    }

                    m_filePath = Path.ChangeExtension(filePath, "rss");
                }
                else
                    Engine.Reflection.Compute.RecordError("Could not understand FilePath.");
            }
        }

        private bool OpenDatabase()
        {
            if (m_Application == null)
            {
                return false;
            }
            m_IDBIO = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);

            int loadOutput = m_IDBIO.LoadDataBase2(m_filePath, "BHoM_UI"); //if 0 successful

            //check if data base is properly loaded
            switch (loadOutput)
            {
                case 0:
                    //Success!
                    break;
                case 25673:
                    Engine.Reflection.Compute.RecordError("Cannot access RAM database. If file is open in RAM, please close. Otherwise, open the file in RAM, close RAM, and try again.");
                    return false;
                case 25657:
                    File.Delete(m_filePath.Replace(".rss", ".usr"));       // Delete usr file
                    Engine.Reflection.Compute.RecordError("RAM Version installed does not match version of file.");
                    return false;
                case 25674:
                    Engine.Reflection.Compute.RecordError(".rss and .ram file exist for same model.");
                    return false;
                case 301:
                    Engine.Reflection.Compute.RecordError("Failed to read .ram file.");
                    return false;
                default:
                    break;
            }

            // Object Model Interface
            m_Model = m_Application.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            return true;
        }

        private bool CloseDatabase()
        {
            if (m_Application == null)
            {
                return false;
            }

            m_IDBIO.CloseDatabase();
            m_IDBIO = null;
            m_Model = null;

            // Delete usr file
            System.IO.File.Delete(m_filePath.Replace(".rss", ".usr"));
            return true;
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        //Add any comlink object as a private field here, example named:

        //private SoftwareComLink m_softwareNameCom;

        private string m_filePath;
        private RamDataAccess1 m_Application;
        private IDBIO1 m_IDBIO;
        private IModel m_Model;

        /***************************************************/

    }
}

