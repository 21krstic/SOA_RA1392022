using System.Text;
using Followers.Api.Data;
using Followers.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Without TLS, Kestrel cannot multiplex HTTP/1.1 and HTTP/2 on the same port
// (no ALPN to negotiate on). REST stays on 8080 (HTTP/1.1); gRPC gets its own
// HTTP/2-only cleartext (h2c) port, 8081.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

builder.Services.Configure<Neo4jSettings>(builder.Configuration.GetSection("Neo4j"));
builder.Services.AddSingleton<FollowsRepository>();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Followers" }));
app.MapControllers();
app.MapGrpcService<FollowersGrpcService>();

app.Run();
