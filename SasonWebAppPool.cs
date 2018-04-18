using Antibiotic.Database.Connectors;
using Antibiotic.TableModels.TStructureModels.DatabaseTables;
using Antibiotic.TableModels.TStructureModels.ServerTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs
{
    public class SasonWebAppPool : SasonBase.SasonBaseApplicationPool
    {
        public SasonWebAppPool()
        {
            this.Parameters.SetParameter("Language", "Turkish");
            this.Parameters.SetParameter("LanguageId", 0m);
        }

        public new static SasonWebAppPool Get
        {
            get { return R.AppPool as SasonWebAppPool; }
        }

        public static new SasonWebAppPool Create
        {
            get
            {
                SasonWebAppPool ret = new SasonWebAppPool();
                ret.EbaTestConnector = SasonBase.SasonConnectorManager.CreateEbaConnectorFromConfigFile();
                ret.EbaTestConnector.OpenConnectionTime = TimeSpan.FromSeconds(5);
                return ret;
            }
        }

        public static new AppPoolMask<SasonWebAppPool> CreateMask
        {
            get
            {
                AppPoolMask<SasonWebAppPool> ret = new AppPoolMask<SasonWebAppPool>();
                if (ret.Custom)
                    ret.AppPool = Create;
                return ret;
            }
        }
    }
}