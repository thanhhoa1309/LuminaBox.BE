using System.Text.Json.Serialization;
using LuminaBox.Api.Architecture;
using LuminaBox.Api.Middlewares;
using LuminaBox.Infrastructure.Realtime;
using Swashbuckle.AspNetCore.SwaggerUI;
using SwaggerThemes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.SetupIocContainer(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.Logger.LogInformation("Starting LuminaBox API...");
app.Logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
app.Logger.LogInformation($"IsDevelopment: {app.Environment.IsDevelopment()}");
try
{
    app.ApplyMigrations(app.Logger);
    app.Logger.LogInformation("Database migrations completed successfully");
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "CRITICAL: Failed to apply database migrations. Application cannot start.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.HeadContent = $"<style>{SwaggerTheme.GetSwaggerThemeCss(Theme.Dracula)}</style>";
        c.EnablePersistAuthorization();
        c.DocExpansion(DocExpansion.List);
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseCors(IocContainer.FrontendCorsPolicy);

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AppHub>(AppHub.Route);

app.Run();
