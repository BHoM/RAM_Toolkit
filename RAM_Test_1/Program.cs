using System;
using System.Collections.Generic;
using RAMDATAACCESSLib;
using RAMDataBaseAccess;

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
            string strWorkingDir;
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
            Stories = new List<int>();
            List<string> ColumnSections = new List<string>();

            // Set filepaths (New can be any filepath, existing has to be an actual model; will give errors is interface has not been released, still working on it)
            filePathNew = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\new_2.rss";
            filePathExisting = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\Tutorial_v1507_US.rss";
            strWorkingDir = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial";
            
            // Usr filepath so we can delete .usr at end of function
            filePathUserfile = filePathExisting.Replace(".rss", ".usr");
            
            // Initialize Data Access
            RAMDataAcc1 = new RamDataAccess1();

            // Set Type (for testing)
            string Type = "Existing";
            //string Type = "Existing";

            RAMDataAccIDBIO = null;

            
            RAMDataAccIDBIO = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IDBIO1_INT);

            
            // Initialize to interface (CREATE NEW MODEL)
            if (Type.Equals("New")) {

                RAMDataAccIDBIO.CreateNewDatabase2(filePathNew, EUnits.eUnitsEnglish, "Grasshopper");

                // Object Model Interface
                IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            }

            // Initialize to interface (FOR EXISTING MODEL)
            if (Type.Equals("Existing")) {

                RAMDataAccIDBIO.LoadDataBase(filePathExisting);
       
                // Object Model Interface
                IModel = RAMDataAcc1.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

                // Get stories
                IStories = IModel.GetStories();
                int numStories = IStories.GetCount();
                Stories.Add(numStories);



                // Get columns on first story
                IColumns = IStories.GetAt(1).GetColumns();
                int numColumns = IColumns.GetCount();

                // Find name of every column (to begin)
                for (int i = 0; i < IColumns.GetCount(); i++)
                {

                    // Get the name of every column
                    IColumn IColumn = IColumns.GetAt(i);
                    string section = IColumn.strSectionLabel;
                    ColumnSections.Add(section);

                }

                //Write output of original database
                Console.WriteLine(filePathExisting);
                Stories.ForEach(i => Console.Write("{0}\t", i));
                ColumnSections.ForEach(i => Console.Write("{0}\t", i));

                // Set every column to a standard section size
                for (int i = 0; i < IColumns.GetCount(); i++)
                {

                    // Set every column to a standard size (do not think this is working yet)
                    IColumn IColumn = IColumns.GetAt(i);
                    IColumn.strSectionLabel = "C12x24";
                    
                }

            }

            //Write output of new database
            Console.WriteLine(filePathExisting);
            Stories.ForEach(i => Console.Write("{0}\t", i));
            ColumnSections.ForEach(i => Console.Write("{0}\t", i));

            // Release main interface and delete user file
            RAMDataAccIDBIO = null;
            System.IO.File.Delete(filePathUserfile);

            int test = 1;
        }






    }
}

