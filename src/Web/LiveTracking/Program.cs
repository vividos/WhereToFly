using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WhereToFly.Web.LiveTracking
{
    /// <summary>
    /// Program for the LiveTracking web page
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Sonar Code Smell",
            "S4823:Using command line arguments is security-sensitive",
            Justification = "ASP.NET Core boilerplate code")]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates host builder for given command line args
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>host builder</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
