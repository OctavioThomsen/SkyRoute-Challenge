using backend.Repositories;
using backend.Services;
using backend.Services.Pricing;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

// CORS: only the SkyRoute frontend origin is permitted.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Pricing strategies (Strategy pattern) — add new providers here without touching existing code.
builder.Services.AddSingleton<IPricingStrategy, GlobalAirPricingStrategy>();
builder.Services.AddSingleton<IPricingStrategy, BudgetWingsPricingStrategy>();
builder.Services.AddSingleton<IPricingService, PricingService>();

builder.Services.AddSingleton<IFlightService, FlightService>();
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Home = "SkyRoute backend up and running"
    });
});

app.Run();
