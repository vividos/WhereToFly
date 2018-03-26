using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace LiveWaypoints
{
    /// <summary>
    /// Main program for WebAPI
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for WebAPI
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Builds web host object
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>built web host object</returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
