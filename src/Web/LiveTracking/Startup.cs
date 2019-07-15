using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WhereToFly.Web.LiveTracking
{
    /// <summary>
    /// Startup class for the LiveTracking web page
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Creates startup class
        /// </summary>
        /// <param name="configuration">configuration to use</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Configuration data
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the
        /// container.
        /// </summary>
        /// <param name="services">service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request
        /// pipeline.
        /// </summary>
        /// <param name="app">app builder</param>
        /// <param name="env">hosting environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
