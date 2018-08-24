using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.RAM
{
    public class Grid
    {
        /***************************************************/
        /****           Public Methods                  ****/
        /***************************************************/

        /// Gridsystem type- Orthogonal, Skewed Orthogonal or Radial 
        public int GridType { get; set; }

        /// Gridsystem Name
        public string GridLabel { get; set; }

        /// Gridsystem offset in X,Y
        public double GridXoffset { get; set; }
        public double GridYoffset { get; set; }

        /// Gridsystem Rotation in Radians
        public double GridRotation { get; set; }
       

     
    }

}
