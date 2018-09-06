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



        public static IGridSystem ToRAM(Grid bhomGrid, IModelGrids IModelGrids, IGridSystems IGridSystems, IGridSystem IGridSystem, int gridCount, string label,  double gridCoord, EGridAxis gridOrientation)
        {

            //initialize a temp Bhom grid object from the object that is passed
            Grid myBhomGrid = bhomGrid;
            int currGrid = gridCount; 
            IGridSystem myGridSystem = IGridSystem;

            IModelGrids = myGridSystem.GetGrids();
            IModelGrid myGridModel = IModelGrids.GetAt(currGrid);
            //int numGridLines = IModelGrids.GetCount();

            //int numGridSystems = IGridSystems.GetCount();

            //Get the name of the Bhom Grid
            string gridSystemName = IGridSystem.strLabel;
            //string gridSystemName = myBhomGrid.Name;


            // get the grid line name from the BHOM grid exists and pass it to RAMobject
            string gridLineLabel = label;

             //get the coordinate or rotation angle of a grid line object
             double gridLineCoord_Angle = gridCoord;

            // get the orientation of the curve
            EGridAxis gridLineDirection = gridOrientation;

            // get minimum and maximum limit specified by the user to which gridline will be drawn from origin
            double dMaxLimit = myGridModel.dMaxLimitValue;
            double dMinLimit = myGridModel.dMinLimitValue;

             myGridModel = IModelGrids.Add(gridLineLabel, gridLineDirection, gridLineCoord_Angle);
           

            

            /*
            ICurve[] gridCrvs = new ICurve[100];
            foreach (ICurve curve in gridCrvs)
            {
                ICurve myGridCurve = curve;

                myGridCurve = bhomGrid.Curve; //take each curve and convert to RAM curve 
                //Get info for each  grid line object
                // get the grid line name from the BHOM grid exists and pass it to RAMobject
                string gridLineLabel = myGridModel.strLabel;


                //get the unique ID of a grid line object
                int gridLinelUID = myGridModel.lUID; 

                //get the coordinate or rotation angle of a grid line object
                double gridLineCoord_Angle = myGridModel.dCoordinate_Angle;

                // get the orientation of the curve
                EGridAxis gridLineDirection = myGridModel.eAxis;

                // get minimum and maximum limit specified by the user to which gridline will be drawn from origin
                double dMaxLimit = myGridModel.dMaxLimitValue;
                double dMinLimit = myGridModel.dMinLimitValue;

                myGridModel = IModelGrids.Add(gridLineLabel, gridLineDirection, gridLineCoord_Angle);

            }
            */


            ///TODO: revise that part, how do I populate the IModelGrid variables?
            ///


            //myGridSystem = (IGridSystem)myGridModel;
            return myGridSystem;


        }

    }

}
