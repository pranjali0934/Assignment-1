using ExpenseAssistant.Api.Config;
using ExpenseAssistant.Api.Middleware;
using ExpenseAssistant.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<GeminiOptions>(options =>
{
    builder.Configuration.GetSection(GeminiOptions.SectionName).Bind(options);
    // GEMINI_API_KEY env var takes precedence over appsettings/User Secrets, per quickstart.md.
    var envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    if (!string.IsNullOrWhiteSpace(envKey))
    {
        options.ApiKey = envKey;
    }
});

builder.Services.AddHttpClient<IGeminiClient, GeminiClient>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<ExtractionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevClient", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExtractionErrorMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AngularDevClient");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
