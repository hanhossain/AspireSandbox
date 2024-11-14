using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddRedisDistributedCache("cache");
builder.AddKafkaProducer<string, string>("kafka");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/weatherforecast", async (IDistributedCache cache, IProducer<string, string> producer) =>
{
    var cachedForecast = await cache.GetAsync("forecast");
    if (cachedForecast is null)
    {
        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
        var forecast = Enumerable.Range(1, 5).Select(index => 
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        await cache.SetAsync("forecast", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(forecast)), new()
        {
            AbsoluteExpiration = DateTime.Now.AddSeconds(10)
        });

        producer.Produce("topic1", new Message<string, string>
        {
            Key = DateTime.Now.ToString("o"),
            Value = "New forecast generated."
        });
        
        return forecast;
    }

    producer.Produce("topic1", new Message<string, string>()
    {
        Key = DateTime.Now.ToString("o"),
        Value = "Cached forecast retrieved."
    });

    return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(cachedForecast);
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
