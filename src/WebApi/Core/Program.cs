using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WhereToFly.WebApi.Core
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Sonar Code Smell",
            "S4823:Using command line arguments is security-sensitive",
            Justification = "ASP.NET Core boilerplate code")]
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
