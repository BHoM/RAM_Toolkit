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

        public int ObjectId { get; set; } = 0;
        public double dist { get; set; } = 0;
        public double DL { get; set; } = 0;
        public double RedLL { get; set; } = 0;
        public double NonRLL { get; set; } = 0;
        public double StorLL { get; set; } = 0;
        public double RoofLL { get; set; } = 0;
        public string type { get; set; } = "";

        /***************************************************/
    }

    /***************************************************/

    public class RAMLineGravityLoad : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public int ObjectId { get; set; } = 0;
        public double dist1 { get; set; } = 0;
        public double dist2 { get; set; } = 0;
        public double DL1 { get; set; } = 0;
        public double DL2 { get; set; } = 0;
        public double LL1 { get; set; } = 0;
        public double LL2 { get; set; } = 0;
        public double PL1 { get; set; } = 0;
        public double PL2 { get; set; } = 0;
        public string type { get; set; } = "";

        /***************************************************/
    }

    /***************************************************/

    public class RAMFactoredEndReactions : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public int ObjectId { get; set; } = 0;
        public NodeReaction StartReaction { get; set; } = null;
        public NodeReaction EndReaction { get; set; } = null;

        /***************************************************/
    }

    /***************************************************/
}
