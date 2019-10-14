using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Loads;

namespace BH.oM.Structure.Loads
{
    public class UniformLoadSet : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Dictionary<Loadcase, double> Loads = null;

        /***************************************************/
    }
}
