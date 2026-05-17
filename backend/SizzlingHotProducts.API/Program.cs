using SizzlingHotProducts.API.Services;
using SizzlingHotProducts.Core.Exceptions;
using SizzlingHotProducts.Core.Interfaces;
using SizzlingHotProducts.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Sizzling Hot Products API",
        Version = "v1",
        Description = "Returns the top-selling (sizzling hot) products for Bunnings, " +
                      "calculated per day and over a 3-day rolling window."
    });
    // Include XML comments if generated
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// Domain services - Singleton is safe because the service is stateless.
builder.Services.AddSingleton<ISizzlingProductService, SizzlingProductService>();

// Data loader - Singleton so the in-memory cache is shared across requests.
builder.Services.AddSingleton<IDataLoader, JsonDataLoader>();

// CORS - allow the React dev server (localhost:5173 is Vite's default).
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevServer", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Pipeline ──────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sizzling Hot Products v1");
        c.RoutePrefix = "swagger";
    });
}

// Global exception handler - returns consistent JSON error shape.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        var message = exceptionFeature?.Error switch
        {
            DataLoadException e => e.Message,
            _ => "An unexpected error occurred."
        };

        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseHttpsRedirection();
app.UseCors("ReactDevServer");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for integration testing (WebApplicationFactory pattern)
public partial class Program { }
