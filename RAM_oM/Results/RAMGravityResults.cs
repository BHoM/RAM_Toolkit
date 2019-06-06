using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Results;
using BH.oM.Geometry;

namespace BH.oM.Structure.Results
{
    public class RAMPointGravityLoad : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public double objectId { get; set; } = 0;
        public double dist { get; set; } = 0;
        public double DL { get; set; } = 0;
        public double RedLL { get; set; } = 0;
        public double NonRLL { get; set; } = 0;
        public double StorLL { get; set; } = 0;
        public double RoofLL { get; set; } = 0;

        /***************************************************/
    }

    public class RAMLineGravityLoad : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public double objectId { get; set; } = 0;
        public double distStart { get; set; } = 0;
        public double distEnd { get; set; } = 0;
        public double DL { get; set; } = 0;
        public double RedLL { get; set; } = 0;
        public double NonRLL { get; set; } = 0;
        public double StorLL { get; set; } = 0;
        public double RoofLL { get; set; } = 0;

        /***************************************************/
    }
}
