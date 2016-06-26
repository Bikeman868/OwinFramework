using System;
using Microsoft.Owin.Hosting;

namespace ExampleUsage
{
    class Program
    {
        /// <summary>
        /// This progran provides a couple of sample configurations that demonstrate 
        /// a very simple use case and a more complex one. There are a limitless number
        /// of possible configurations so these are just a couple of examples
        /// </summary>
        static void Main(string[] args)
        {
            var opt = args.Length > 0 ? args[0] : "simple";
            var url = args.Length > 1 ? args[1] : "http://localhost:12345";

            Action waitForExit = () =>
            {
                Console.WriteLine("Owin framework application listening on " + url);
                Console.WriteLine("Press any key to stop.");
                Console.ReadLine();
            };

            try
            {
                switch (opt)
                {
                    case "simple":
                        using (WebApp.Start<StartupSimple>(url))
                        {
                            waitForExit();
                        }
                        break;

                    case "routing":
                        using (WebApp.Start<StartupRouting>(url))
                        {
                            waitForExit();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception. Press any key to exit.");
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                Console.ReadLine();
            }
        }
    }

}
