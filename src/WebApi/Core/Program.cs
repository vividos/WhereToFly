using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WhereToFly.WebApi.LiveWaypoints
{
    /// <summary>
    /// Main program for WebAPI
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for WebAPI
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Returns a web host builder object
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>built web host object</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
