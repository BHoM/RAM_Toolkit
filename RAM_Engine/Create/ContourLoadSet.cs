using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Loads;
using BH.oM.Adapters.RAM;
using RAMDATAACCESSLib;
using BH.Engine.Reflection;
using BH.oM.Geometry;


namespace BH.Engine.Adapters.RAM
{
    public static partial class Create
    {
        public static ContourLoadSet CreateContourLoadSet(UniformLoadSet loadSet, Polyline contour, string name = "")
        {

            return new ContourLoadSet
            {
                UniformLoadSet = loadSet,
                Contour = contour,
                Name = name
            };            

            /***************************************************/
        }
    }
}
