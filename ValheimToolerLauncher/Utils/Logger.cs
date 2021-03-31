using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimToolerLauncher.Utils
{
    class Logger
    {
        public static void Log(string message)
        {
            using (StreamWriter w = File.AppendText("crash_report.txt"))
            {
                w.WriteLine("\r\n{0} {1} :", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                w.WriteLine(message);
                w.WriteLine("-------------------------------");
            }
        }
    }
}
