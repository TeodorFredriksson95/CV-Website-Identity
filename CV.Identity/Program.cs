using CV.Identity;
using CV.Identity.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

string connectionString = Environment.GetEnvironmentVariable("unidevwebcon")!;
var jwtTokenSecret = Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET", EnvironmentVariableTarget.Process);
var jwtConfigIssuer = Environment.GetEnvironmentVariable("IDENTITY_JWTCONFIG_ISSUER", EnvironmentVariableTarget.Process);
var jwtConfigAudience = Environment.GetEnvironmentVariable("IDENTITY_JWTCONFIG_AUDIENCE", EnvironmentVariableTarget.Process);
builder.Services.AddApplication();

Console.WriteLine("CONNECTION STRING: " + connectionString);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDatabase(connectionString);


builder.Services.AddJWTService(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenSecret)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtConfigIssuer,
        ValidAudience = jwtConfigAudience,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.FromSeconds(0),

    };
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation($"Connection string: CON STRING SHOULD BE HERE:::{connectionString}");
logger.LogInformation($"HERE ARE OTHER ENV VARIABLES: secret{jwtTokenSecret}, issuer {jwtConfigIssuer}");

//app.UseMiddleware<RequestLoggingMiddleware>();
//app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();
app.Run();
