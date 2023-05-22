using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePay
{
    public class Log
    {
        private string PathName { get; }
        public Log(string pathName)
        {
            PathName = pathName;
        }

        public void WriteLine(string msg, bool logToFile = true)
        {
            Console.WriteLine(msg);

            if (!logToFile)
                return;

            using (var sw = new StreamWriter(PathName))
            {
                sw.WriteLine(msg);
            }
        }

        public void AppendLine(string msg, bool logToFile = true)
        {
            Console.WriteLine(msg);

            if (!logToFile)
                return;

            using (var sw = new StreamWriter(PathName, true))
            {
                sw.WriteLine(msg);
            }
        }
    }
}
