using System;
using PAC.Servers;

namespace PAC
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var url = "http://localhost:8080/";
            if (args.Length > 0)
                url = args[0];

            using (var server = MainServer.CreateWebServer(url))
            {
                server.RunAsync();

                var browser = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }
                };
                browser.Start();
                Console.ReadKey(true);
            }
        }
    }
}
