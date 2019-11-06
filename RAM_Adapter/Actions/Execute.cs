using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.RAM
{
    public abstract partial class RAMAdapter
    {
        public override bool Execute(string command, Dictionary<string, object> parameters = null, Dictionary<string, object> config = null)
        {
            command = command.ToUpper();

            switch (command)
            {
                case ("CLOSE"):
                    {
                        if (RAMDataAccIDBIO != null)
                        {
                            RAMDataAccIDBIO.SaveDatabase();
                            RAMDataAccIDBIO.CloseDatabase();
                        }

                        RAMDataAccIDBIO = null;
                        m_RAMApplication = null;

                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
