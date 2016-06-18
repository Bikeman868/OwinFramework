using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin.Hosting;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:12345";

            try
            {
                using (WebApp.Start<Startup>(url))
                {
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
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
