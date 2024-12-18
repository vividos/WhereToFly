using WhereToFly.Web.App.Components;

/// <summary>
/// Web app program
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">command line args</param>
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
