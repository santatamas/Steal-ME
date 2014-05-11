namespace StealME.Server.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class SMLogger
    {
        public static void LogThis(string message)
        {
            string logMessage = "[StealME.Service-" + DateTime.Now.ToString() + "] " + message;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Debug.WriteLine(logMessage);
            Console.WriteLine(logMessage);

            using (FileStream fs = File.Open("log.txt", FileMode.Append,FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(logMessage);
                sw.Close();
            }
        }
    }
}
