using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Loads;

namespace BH.oM.Adapters.RAM
{ 
    public class UniformLoadSet : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Dictionary<string, Loadcase> Loadcases { get; set; } = null;
        public Dictionary<string, double> Loads { get; set; } = null;

        /***************************************************/
    }
}
