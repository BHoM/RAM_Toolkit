using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Loads;

namespace BH.Engine.Structure.Loads
{
    public static partial class Create
    {
        public static UniformLoadSet CreateRAMUniformLoadSet(string name, double sdl, double cdl, double llReducible, double llNonreducible, double partition, double cll, double massDl)
        {
            //This section, definition of RAM internal loadcases, needs to go somewhere central and be referenced here.
            Loadcase sdlCase = new Loadcase { Name = "SDL", Nature = LoadNature.SuperDead };
            Loadcase cdlCase = new Loadcase { Name = "CDL", Nature = LoadNature.Dead };
            Loadcase llReducibleCase = new Loadcase { Name = "LLRed", Nature = LoadNature.Live };
            Loadcase llNonreducibleCase = new Loadcase { Name = "LLNRed", Nature = LoadNature.Live };
            Loadcase partitionCase = new Loadcase { Name = "Partition", Nature = LoadNature.Live };
            Loadcase cllCase = new Loadcase { Name = "CLL", Nature = LoadNature.Live };
            Loadcase massDlCase = new Loadcase { Name = "MassDL", Nature = LoadNature.Dead };

            return new UniformLoadSet
            {
                Name = name,
                Loads = new Dictionary<Loadcase, double>
                {
                    { sdlCase, sdl },
                    { cdlCase, cdl },
                    { llReducibleCase, llReducible },
                    { llNonreducibleCase, llNonreducible },
                    { partitionCase, partition },
                    { cllCase, cll },
                    { massDlCase, massDl }
                }
            };

            /***************************************************/
        }
    }
}
