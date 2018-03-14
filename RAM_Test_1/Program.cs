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

            // Set filepaths (New can be any filepath, existing has to be an actual model)
            filePathNew = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\new_2.rss";
            filePathExisting = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial\\Tutorial_v1506_US_4.rss";
            strWorkingDir = "C:\\ProgramData\\Bentley\\Engineering\\RAM Structural System\\Data\\Tutorial";

            // Initialize Data Access
            RAMDataAcc1 = new RamDataAccess1();

            // Set Type (for testing)
            string Type = "New";
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

                for (int i = 0; i < IColumns.GetCount(); i++)
                {

                    // Right now this does not actually print the Section Label, just an object
                    string section = IColumns.GetAt(i).GetProperty("strSectionLabel").ToString();
                    ColumnSections.Add(section);

                }

                //Write output (for testing)
                Console.WriteLine(filePathExisting);
                Stories.ForEach(i => Console.Write("{0}\t", i));
                ColumnSections.ForEach(i => Console.Write("{0}\t", i));

            }

            //Write output (for testing)
            Console.WriteLine(filePathExisting);
            Stories.ForEach(i => Console.Write("{0}\t", i));
            ColumnSections.ForEach(i => Console.Write("{0}\t", i));

            // Release main interface
            RAMDataAccIDBIO = null;

            int test = 1;
        }






    }
}

