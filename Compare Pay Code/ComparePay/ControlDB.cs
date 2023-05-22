using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePay
{
    public class ControlDB
    {
        public string HostName { get; set; }
        public string DatabaseName { get; set; }

        public ControlDB(string spec)
        {
            if (String.IsNullOrEmpty(spec))
                return;

            string[] tokens = spec.Split(';');
            if (tokens.Length == 2)
            {
                HostName = tokens[0];
                DatabaseName = tokens[1];
            }
        }

        public string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(HostName) || String.IsNullOrEmpty(DatabaseName))
                    return "";

                //return $"provider=sqloledb;server={HostName};database={DatabaseName};uid=wbpoc;pwd=sql@tfs2008";
                return $"server={HostName};database={DatabaseName};uid=wbpoc;pwd=sql@tfs2008";
            }
        }

        public string GetInstanceDBConnectionString(string instanceNamespace)
        {
            string GET_NAMESPACE_CONNECT_STRING =
                $@"
                    SELECT DatabaseConnectionString FROM DatabaseConnection WHERE Name = '{instanceNamespace}'
                ";
            var sqlUtil = new SqlUtil(ConnectionString);
            return sqlUtil.GetValue<string>(GET_NAMESPACE_CONNECT_STRING);
        }
    }

}
