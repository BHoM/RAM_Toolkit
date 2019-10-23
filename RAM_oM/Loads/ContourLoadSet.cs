using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.RAM
{ 
    public class ContourLoadSet : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Polyline Contour { get; set; } = null;

        public UniformLoadSet UniformLoadSet { get; set; } = null;

        /***************************************************/
    }
}
