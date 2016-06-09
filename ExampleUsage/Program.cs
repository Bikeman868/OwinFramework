using System;
using Microsoft.Owin.Hosting;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage
{
    class Program
    {
        /// <summary>
        /// This progran provides a couple of sample configurations that demonstrate 
        /// a very simple use case and a more complex one. There are a limitless number
        /// of possible configurations so these are just a couple of illustrations
        /// </summary>
        static void Main(string[] args)
        {
            var opt = args.Length > 0 ? args[0] : "simple";
            const string url = "http://localhost:12345";

            try
            {
                switch (opt)
                {
                    case "simple":
                        using (WebApp.Start<StartupSimple>(url))
                        {
                            Console.ReadLine();
                        }
                        break;

                    case "routing":
                        using (WebApp.Start<StartupRouting>(url))
                        {
                            Console.ReadLine();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }
        }
    }

}
