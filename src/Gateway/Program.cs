using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// The frontend is a static site on its own origin; allow it (and local
// development from any port) to call the gateway.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();

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
