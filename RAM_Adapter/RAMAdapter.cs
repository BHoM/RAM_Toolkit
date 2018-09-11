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





                //CASE01:  if a filepath to an .rss file is provided either as a explicit filepath or wiith a component
                // Initialize to interface (CREATE NEW MODEL at provided filepath)
                if (filePath!= "")
                //   if (filePath == "" && !File.Exists(filePath))
                {
                    //string defaultPath = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\BHoM_Model.rss";
                    string filePathNew = filePath.Replace("\\\\", "\\");
                    //string filePaPath.GetFullPath(filePath);
                    filePathNew = filePathNew.Replace("\r\n", "");
                    string fileName = Path.GetFileName(filePathNew);
                   
                    filePath = filePathNew;
                    string filePathTempRAMFile0 = Path.GetFullPath(fileName);
                    string fileNameRAM = fileName.Replace(".rss", ".ram");
                
                    string filePathTempRAMFile1 = filePathTempRAMFile0.Replace(fileName, "");

                    filePathTempRAMFile0 = filePathTempRAMFile1 + fileNameRAM;
                    if (File.Exists(filePathTempRAMFile0))
                    {
                        File.Delete(filePathTempRAMFile0);
                    }

   
                    try
                    {
                        RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
 
                        // Object Model Interface               
                        //RAMDataAccIDBIO.CreateNewDatabase2(filePathNew, EUnits.eUnitsEnglish, "Grasshopper");
                        double loadOutput = RAMDataAccIDBIO.LoadDataBase2(filePathNew, "Grasshopper");
                        IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT); 
                        // Delete usr file
    

                    }
                    catch
                    {
                        Console.WriteLine("Cannot create RAM database, check that a compatible version of RAM is installed");
                    }
          

                }

                //CASE02 :  if NO filepath is proivded and NO .rss file exists
                // Initialize to interface (CREATE NEW MODEL in RAM data folder by default)

                if (filePath != "" && !File.Exists(filePath) )
                {
                    //string filePathNew = "Q:\\BHLA Comp Collective Dev\\RAM_test\\BHoM_Model.rss";
                    string filePathNew = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\BHoM_Model.rss";
                    string filePathOld = filePath;

                    try
                    {
                        RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);

                        // Object Model Interface
                        IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
                        RAMDataAccIDBIO.CreateNewDatabase2(filePathNew, EUnits.eUnitsEnglish, "Grasshopper");
                        //filePath = filePathNew; 
                        // Delete usr file
                        File.Delete(filePathNew.Replace(".rss", ".usr"));

                    }
                    catch
                    {
                        Console.WriteLine("Cannot create RAM database, check that a compatible version of RAM is installed");
                    }

                }


                    //case03
                    // if an .rss file is provided
                    // Initialize to interface (OF EXISTING MODEL)

                    if (File.Exists(filePath))
                    {
                        //Delete .ram file in working directory if it exists
                        string fileName = Path.GetFileName(filePath);
                        string fileNameRAM = fileName.Replace(".rss", ".ram");
                        //Two possible working dirs depending on install
                        string filePathWorkingDir1 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Working\\";
                        string filePathWorkingDir2 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Working\\";
                        string filePathTempRAMFile1 = filePathWorkingDir1 + fileNameRAM;
                        string filePathTempRAMFile2 = filePathWorkingDir2 + fileNameRAM;
                        if (File.Exists(filePathTempRAMFile1))
                        {
                            File.Delete(filePathTempRAMFile1);
                        }
                        if (File.Exists(filePathTempRAMFile2))
                        {
                            File.Delete(filePathTempRAMFile2);
                        }

                        RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);
                        double loadOutput = RAMDataAccIDBIO.LoadDataBase2(filePath, "Grasshopper");
  
                    //check if data base is properly loaded
                    if (loadOutput == 25673)
                        {
                            throw new ArgumentException("Cannot access RAM database. Please open the file in RAM, close RAM, and try again.");
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




                /*
               // if an EMPTY filepath is proivded and NO  .rss file exists
                // Initialize to interface (CREATE NEW MODEL in RAM data folder by default)
                //case01
                if (filePath == "" && File.Exists(filePath))
                {
                    //Delete .ram file in working directory if it exists
                    string fileName = Path.GetFileName(filePath);
                    string fileNameRAM = fileName.Replace(".rss", ".ram");
                    //Two possible working dirs depending on install
                    string filePathWorkingDir1 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Working\\";
                    string filePathWorkingDir2 = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\";
                    string filePathTempRAMFile1 = filePathWorkingDir1 + fileNameRAM;
                    string filePathTempRAMFile2 = filePathWorkingDir2 + fileNameRAM;
                    if (File.Exists(filePathTempRAMFile1))
                    {
                        File.Delete(filePathTempRAMFile1);
                    }
                    if (File.Exists(filePathTempRAMFile2))
                    {
                        File.Delete(filePathTempRAMFile2);
                    }

                    RAMDataAccIDBIO = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);

                    double loadOutput = RAMDataAccIDBIO.LoadDataBase2(filePath, "Grasshopper");
                    //check if data base is properly loaded


                    if (loadOutput == 25673)
                    {
                        throw new ArgumentException("Cannot access RAM database. Please open the file in RAM, close RAM, and try again.");
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

                 

                */

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
