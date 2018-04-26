using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

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

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Title = "WhereToFly Live Waypoints API",
                        Version = "v1"
                    });

                // Set the comments path for the Swagger JSON and UI.
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WhereToFly.WebApi.LiveWaypoints.xml");
                c.IncludeXmlComments(xmlPath);
            });
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

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    const string SwaggerUrl = "/swagger/v1/swagger.json";
                    c.SwaggerEndpoint(SwaggerUrl, "My API V1");
                });
            }

            app.UseMvc();
        }
    }
}
