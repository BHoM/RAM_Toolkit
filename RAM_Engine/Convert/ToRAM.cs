using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.oM.Common;
using BH.oM.Structure.Loads;
using BH.Engine.Structure;
using BH.oM.Structure.Properties;
using RAMDATAACCESSLib;
using BH.oM.Architecture.Elements;


namespace BH.Engine.RAM
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        //Add methods for converting From BHoM to the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static RAMNode ToRAM(this Node node)
        //{
        //    //Insert code for convertion
        //}
        /***************************************************/


        //public static IBeam ToRAM(Bar bar)
        //{
        //    IBeam IBeam;
        //    return IBeam;
        //}

        public static ILayoutBeam ToRAM(Bar bar, ILayoutBeams ILayoutBeams)
        {
            ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(0);

            double xStart = bar.StartNode.Position.X;
            double yStart = bar.StartNode.Position.Y;
            double xEnd = bar.EndNode.Position.X;
            double yEnd = bar.EndNode.Position.Y;

            //Set support coordinates and name
            //CAUTION: different from actual end points and cantilevers hardcoded
            ILayoutBeam.SetLayoutCoordinates(xStart, yStart, 0, xEnd, yEnd, 0, 0, 0);
            ILayoutBeam.strSectionLabel = bar.SectionProperty.Name;

            return ILayoutBeam;
        }

        public static SCoordinate ToRAM(Point point)
        {
            SCoordinate Point = new SCoordinate();
            Point.dXLoc = point.X;
            Point.dYLoc = point.Y;
            Point.dZLoc = point.Z;
            return Point;
        }

        public static EMATERIALTYPES ToRAM(oM.Common.Materials.Material material)
        {
            EMATERIALTYPES Material = new EMATERIALTYPES();
            
            if (material.Type == oM.Common.Materials.MaterialType.Concrete) { Material = EMATERIALTYPES.EConcreteMat; }
            else if (material.Type == oM.Common.Materials.MaterialType.Steel) { Material = EMATERIALTYPES.ESteelMat; }
            else { Material = EMATERIALTYPES.ESteelMat; }
            return Material;
        }

        public static string ToRAM(string BHoMSectionName)
        {
            string RAMSecName = BHoMSectionName;
            return RAMSecName;
        }

        /*
        public static EGridAxis ToRAM(ICurve curve)
        {
            ICurve gridCrv = curve;
            // get the orintation of the curve
        
            EGridAxis crvAxis = (0,1,0);  
            return crvAxis; 
        }

        */

           

        public static IGridSystem ToRAM(List<Grid> bhomGrids, IModelGrids IModelGrids, IGridSystem IGridSystem)
        {

            //initialize a temp Bhom grid object from the object that is passed
            //Grid myBhomGrid = bhomGrid;
            List<Grid> myBhomGrids = bhomGrids;
            IGridSystem myGridSystem = IGridSystem;
            IModelGrids myModelGrids = IModelGrids;
            

            //HOW DO I EXTRACT THE CURVES OF A GRID OBJECT?
            //Get the name of the Bhom Grid
            //string gridSystemName = myBhomGrid.Name;
            //ICurve myGridCurve = myBhomGrid.Curve;
            int numGridLines = myModelGrids.GetCount();

            for (int i = 0; i < myBhomGrids.Count(); i++)
            {
                IModelGrid myGridModel = IModelGrids.GetAt(i);
                string gridLabel = myBhomGrids[i].Name;
                myGridModel.strLabel = gridLabel;
            }



            /*
            // Create an array of curves to store gridlines
            //Loop through array to create BHOMcurves 
            ICurve[] gridCrvs = new ICurve[100];

            foreach (ICurve curve in gridCrvs)
            {
                ICurve myGridCurve = curve;
                myGridCurve = bhomGrid.Curve; //take each curve and convert to RAM curve
                string myGridName = myGridCurve.ToString();

            }
            // myGridModel = IModelGrids.Add(gridName, gridDirection, gridRotation);
            */
            return myGridSystem;
        }

    }

}
