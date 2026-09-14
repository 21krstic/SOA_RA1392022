using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Ocelot's middleware sits ahead of the implicit endpoint-execution stage in
// minimal hosting, so it would otherwise intercept /health before it's matched.
// Using explicit UseRouting/UseEndpoints places health-check handling first.
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway" }));
});

await app.UseOcelot();

app.Run();
