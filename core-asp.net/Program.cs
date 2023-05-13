using System.Text.Json;
using StackExchange.Redis;
using SharpPcap;
using PacketDotNet;

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

await Task.Run(() =>
{
    // Retrieve the device list
    var devices = CaptureDeviceList.Instance;

    // If no devices were found, print an error
    if (devices.Count < 1)
    {
        Console.WriteLine("No devices were found on this machine");
        return;
    }
    devices.ToList().ForEach(d => Console.WriteLine(d.Description));
    // Take the first device from the list
    var device = devices[6];

    Console.WriteLine($"-- Using {device.Name} --");

    // Open the device
    device.Open();

    // Register our handler function to the 'packet arrival' event
    device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

    // Start the capturing process
    device.StartCapture();

    // Wait for one second
    System.Threading.Thread.Sleep(1000);

    // Stop the capturing process
    //device.StopCapture();

    // Close the pcap device
    //device.Close();
});

app.Run();

// Callback function invoked by SharpPcap for every incoming packet
// Callback function invoked by SharpPcap for every incoming packet
static void device_OnPacketArrival(object sender, PacketCapture e)
{
    var packet = PacketDotNet.Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);

    //Console.WriteLine(packet.ToString());

    //print the strucutre of the packet
    Console.WriteLine(packet.ToString(StringOutputType.VerboseColored));

}
