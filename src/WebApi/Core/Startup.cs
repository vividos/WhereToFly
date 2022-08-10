using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using WhereToFly.WebApi.Logic;
using WhereToFly.WebApi.Logic.Services;
using WhereToFly.WebApi.Logic.TourPlanning;

namespace WhereToFly.WebApi.LiveWaypoints
{
    /// <summary>
    /// Startup class for WebAPI project
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// CORS policy name for debug mode
        /// </summary>
        private const string DebugCorsPolicyName = "DebugCorsPolicy";

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
            services.AddCors(options =>
            {
                options.AddPolicy(
                    DebugCorsPolicyName,
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            services.AddControllers()
                .AddNewtonsoftJson();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "WhereToFly Web API",
                        Version = "v1"
                    });

                // Set the comments path for the Swagger JSON and UI.
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WhereToFly.WebApi.Core.xml");

                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            AddLogicServices(services);
        }

        /// <summary>
        /// Adds business logic objects to service collection
        /// </summary>
        /// <param name="services">service collection</param>
        private static void AddLogicServices(IServiceCollection services)
        {
            services.AddSingleton<GarminInreachDataService>();
            services.AddSingleton<LiveWaypointCacheManager>();
            services.AddSingleton<LiveTrackCacheManager>();
            services.AddSingleton<LocationFindManager>();

            // confiugre and add tour planning engine
            var logicAssembly = typeof(PlanTourEngine).Assembly;
            var kmlStream = logicAssembly.GetManifestResourceStream("WhereToFly.WebApi.Logic.Assets.PlanTourPaths.kml");

            var engine = new PlanTourEngine();
            engine.LoadGraph(kmlStream);

            services.AddSingleton(engine);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request
        /// pipeline.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">web host environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                    c.SwaggerEndpoint(SwaggerUrl, "WhereToFly Web API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseCors(DebugCorsPolicyName);
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
