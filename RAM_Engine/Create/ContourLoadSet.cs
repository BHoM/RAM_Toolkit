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


namespace BH.Engine.Adapters.RAM
{
    public static partial class Create
    {
        public static ContourLoadSet CreateContourLoadSet(string name, UniformLoadSet loadSet)
        {

            return new ContourLoadSet
            {
                Name = name,
                UniformLoadSet = loadSet,
            };            

            /***************************************************/
        }
    }
}
