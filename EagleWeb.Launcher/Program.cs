using System;
using System.Collections.Generic;
using System.Threading;

namespace EagleWeb.Launcher
{
    class Program
    {
        static void Main(string[] rawArgs)
        {
            //Parse arguments
            if (!ParseArgs(rawArgs, out Dictionary<string, string> args))
                return;

            //Get required arguments
            string eagleDir;
            string execPath;
            if (!args.TryGetValue("eagle-directory", out eagleDir) || !args.TryGetValue("exec-path", out execPath))
            {
                Console.WriteLine("Missing one or more required arguments: eagle-directory, exec-path");
                return;
            }

            //Create application
            app = new EagleApplication(new System.IO.FileInfo(execPath), eagleDir, eagleDir);

            //Create optional components
            if (args.ContainsKey("developer"))
            {
                dev = new EagleDeveloperServer(app);
                Console.WriteLine($"Running developer server on :{dev.Port}.");
            }

            //Start
            Console.WriteLine("Launching application...press enter to stop.");
            Console.WriteLine("============================================");
            app.Start();
            Console.ReadLine();
            app.Stop();
        }

        private static EagleApplication app;
        private static EagleDeveloperServer dev;

        /// <summary>
        /// Very primitive commandline parsing function. Returns true on success.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        static bool ParseArgs(string[] args, out Dictionary<string, string> result)
        {
            result = new Dictionary<string, string>();
            string last = null;
            foreach (var v in args)
            {
                //Determine if this is a key or a value
                if (v.StartsWith("--"))
                {
                    //Key
                    last = v.Substring(2);
                    if (result.ContainsKey(last))
                    {
                        Console.WriteLine($"Malformed argument. Argument \"{v}\" was duplicated.");
                        return false;
                    }
                    result.Add(last, null);
                } else if (last == null)
                {
                    //Value, but in an invalid location
                    Console.WriteLine($"Malformed arguments. Check argument \"{v}\".");
                    return false;
                } else
                {
                    //Value
                    result[last] = v;
                    last = null;
                }
            }
            return true;
        }
    }
}
