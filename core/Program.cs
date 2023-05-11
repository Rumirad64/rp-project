using System;
using System.Net;
using System.Threading.Tasks;

class Program
{
    public static async Task Main()
    {
        string[] prefixes = { "http://*:15000/" }; // Add more prefixes here as needed

        HttpListener listener = new HttpListener();

        foreach (string s in prefixes)
        {
            listener.Prefixes.Add(s);
        }

        listener.Start();
        Console.WriteLine("Listening on prefixes: {0}", string.Join(", ", prefixes));

        try
        {
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = ProcessRequestAsync(context);
                Console.WriteLine("Received request from: {0}", context.Request.RemoteEndPoint);
            }
        }
        finally
        {
            listener.Close();
        }
    }

    public static async Task ProcessRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;

        Console.WriteLine("\nReceived request:");
        Console.WriteLine($"URL: {request.Url}");
        Console.WriteLine($"Method: {request.HttpMethod}");
        Console.WriteLine("HTTP Headers:");

        foreach (string header in request.Headers)
        {
            Console.WriteLine($"{header}: {request.Headers[header]}");
        }

        HttpListenerResponse response = context.Response;
        string responseString = "<html><body>Request received!</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        await output.WriteAsync(buffer, 0, buffer.Length);

        output.Close();
    }
}
