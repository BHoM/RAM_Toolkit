using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structural.Elements;
using BH.oM.Geometry;
using BH.oM.Common;
using BH.oM.Structural.Loads;
using BH.Engine.Structure;
using BH.oM.Structural.Properties;
using RAMDATAACCESSLib;

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

    }
}
