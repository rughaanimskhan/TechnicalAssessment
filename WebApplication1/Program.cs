using WebApplication1.Services;
using WebApplication1.Extensions;
using Serilog;
using Polly;
using Polly.Extensions.Http;
using WebApplication1.IServices;
using Microsoft.OpenApi.Models;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Define the retry policy
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(
            retryCount: 5, // Number of retries
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
            });
}

// Configure Circuit Breaker Policy
IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Handles 5xx, 408, and network-related exceptions
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3, // Number of failures before breaking the circuit
            durationOfBreak: TimeSpan.FromSeconds(30), // Duration to keep the circuit open
            onBreak: (exception, breakDelay) =>
            {
                Console.WriteLine($"Circuit broken! Exception: {exception.Exception.Message}. Break duration: {breakDelay.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit reset! API is healthy again.");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine("Circuit in half-open state. Testing API health...");
            });
}

// Add HttpClient with the retry policy
builder.Services.AddHttpClient("MyHttpClient", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Optional: Set handler lifetime
.AddPolicyHandler(GetRetryPolicy()) // Attach the retry policy
.AddPolicyHandler(GetCircuitBreakerPolicy()); // Attach circuit 

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient(); 
builder.Services.AddSingleton<ICurrencyProviderFactory, CurrencyProviderFactory>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.AddJwtBearer(); //JWT settings

var app = builder.Build();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
