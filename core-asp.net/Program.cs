using System.Text.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
IDatabase db = redis.GetDatabase();

// MapFallback is a catch-all route handler. If the request doesn't match any other routes, it will be handled here.
app.MapFallback(async context =>
{
    context.Response.Cookies.Append("name", "value");
    await context.Response.WriteAsJsonAsync(new { timestamp = DateTime.Now });


});

//add middle ware for all requests
app.Use(async (context, next) =>
{
    Console.WriteLine("\nReceived request:");
    Console.WriteLine($"URL: {context.Request.Path}");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine("HTTP Headers:");

    var headers = new Dictionary<string, string>();

    foreach (var header in context.Request.Headers)
    {
        headers[header.Key] = header.Value;
        Console.WriteLine($"{header.Key}: {header.Value}");
    }

    // Convert the headers dictionary to a JSON string.
    string json = JsonSerializer.Serialize(headers);

    TimeSpan pong = await db.PingAsync();

    // write the latency to header for each request
    context.Response.Headers.Add("X-Redis-Latency", pong.TotalMilliseconds.ToString());
    // Push the JSON string to a Redis 
    await db.PublishAsync("requests", json);

    await next.Invoke();
});

app.Run();
