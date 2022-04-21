using Microsoft.AspNetCore.StaticFiles;

namespace WhereToFly.Web.LiveTracking;

/// <summary>
/// Program for the LiveTracking web page
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">command line args</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Code Smell",
        "S4823:Using command line arguments is security-sensitive",
        Justification = "ASP.NET Core boilerplate code")]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddAntiforgery();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".czml"] = "application/json";
        app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

        app.UseRouting();

        app.MapRazorPages();

        app.Run();
    }
}
