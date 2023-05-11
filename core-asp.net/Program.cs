using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => {
    return new { Message = "Hello World!" };
});

app.Run();
void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.Use(async (context, next) =>
    {
        System.Diagnostics.Debug.WriteLine("\nReceived request:");
        System.Diagnostics.Debug.WriteLine($"URL: {context.Request.Path}");
        System.Diagnostics.Debug.WriteLine($"Method: {context.Request.Method}");
        System.Diagnostics.Debug.WriteLine("HTTP Headers:");
        Debug.Print("-------------");


        foreach (var header in context.Request.Headers)
        {
            Debug.WriteLine($"{header.Key}: {header.Value}");
        }

        await next.Invoke();
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Request received!");
        });
    });
}
