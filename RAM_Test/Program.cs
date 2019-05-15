using System;
using System.Collections.Generic;
using System.Linq;
using BH.Adapter.FileAdapter;
using BH.Adapter.RAM;
using BH.oM.Architecture.Elements;
using BH.oM.DataManipulation.Queries;
using BH.oM.Structure.Elements;



namespace RAM_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Writing RAM File");
            RAMAdapter app = new RAMAdapter(@"C:\Users\jtaylor\OneDrive - BuroHappold\Tools in Progress\BHoM Tests\RAM\RAM Copy Out 3.rss", true);
            FileAdapter doc = new FileAdapter("C:/Users/jtaylor/GitHub/RAM_Toolkit/RAM_Test", "Test_Structure", true);

            FilterQuery levelQuery = new FilterQuery { Type = typeof(Level) };
            FilterQuery barQuery = new FilterQuery { Type = typeof(Bar) };
            FilterQuery panelQuery = new FilterQuery { Type = typeof(PanelPlanar) };

            IEnumerable<object> levels = doc.Pull(levelQuery);
            IEnumerable<object> bars = doc.Pull(barQuery);
            IEnumerable<object> panels = doc.Pull(panelQuery);

            int numPushed = bars.Count() + panels.Count();


            IEnumerable<BH.oM.Base.BHoMObject> levelObjects = (IEnumerable<BH.oM.Base.BHoMObject>)levels;
            IEnumerable<BH.oM.Base.BHoMObject> barObjects = (IEnumerable<BH.oM.Base.BHoMObject>)bars;
            IEnumerable<BH.oM.Base.BHoMObject> panelObjects = (IEnumerable<BH.oM.Base.BHoMObject>)panels;

            app.Push(levelObjects, "");
            app.Push(barObjects, "");
            //app.Push(panelObjects, "");

            //IEnumerable<object> barsPulled = app.Pull(barQuery);
            //IEnumerable<object> panelsPulled = app.Pull(panelQuery);

            //int numPulled = barsPulled.Count() + panelsPulled.Count();

            //Console.WriteLine("Pushed " + numPushed + " Objects, pulled " + numPulled + " Objects.");
            ////foreach (Bar bar in barsPulled)
            ////{
            ////    Console.WriteLine("Bar with ID " + bar.CustomData["SAP2000_id"].ToString() + " and property " + bar.SectionProperty.Name);
            ////}
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Console.WriteLine("");
        }

        /***************************************************/
    }
}
