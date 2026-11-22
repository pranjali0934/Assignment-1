using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Middleware;
using ExpenseTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ExpenseTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExpenseTracker")));

builder.Services.AddScoped<ExpenseValidationService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<SummaryService>();

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

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AngularDevClient");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
