using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveWaypoints
{
    /// <summary>
    /// Startup class for WebAPI project
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Starts up WebAPI project
        /// </summary>
        /// <param name="configuration">configuration object</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Configuration object
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the
        /// container.
        /// </summary>
        /// <param name="services">service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request
        /// pipeline.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">hosting environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
