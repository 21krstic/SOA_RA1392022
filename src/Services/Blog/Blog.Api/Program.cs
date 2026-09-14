using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BlogService.Data;
using BlogService.Services;

// Required for Grpc.Net.Client to call a gRPC server over plaintext HTTP/2 (h2c),
// since there is no TLS between services inside the docker network.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<BlogsRepository>();
builder.Services.AddSingleton<CommentsRepository>();
builder.Services.AddSingleton<FollowersServiceClient>();

builder.Services.AddHttpClient<FollowersRestClient>(client =>
{
    var baseUrl = builder.Configuration["Followers:BaseUrl"]
        ?? throw new InvalidOperationException("Followers:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Blog" }));
app.MapControllers();

app.Run();
