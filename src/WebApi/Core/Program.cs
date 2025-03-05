using WhereToFly.Shared.Model.Serializers;
using WhereToFly.WebApi.Core.Services;
using WhereToFly.WebApi.Logic;
using WhereToFly.WebApi.Logic.Services;

const string DebugCorsPolicyName = "DebugCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container
builder.Services.AddSingleton<GarminInreachDataService>();
builder.Services.AddSingleton<LiveWaypointCacheManager>();
builder.Services.AddSingleton<LiveTrackCacheManager>();
builder.Services.AddSingleton<LocationFindManager>();
builder.Services.AddSingleton<PlanTourService>();

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Add(
            SharedModelJsonSerializerContext.Default);
    });

builder.Services.AddCors(
    options =>
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors(DebugCorsPolicyName);
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "WhereToFly Web API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();

/// <summary>
/// Web API program
/// </summary>
public partial class Program
{
}
