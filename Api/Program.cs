using System.Text;
using Api.Clients;
using Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Api.Services;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddSingleton<ApiMqttClient>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GreenhouseService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<PresetService>();



builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<SensorReadingsService>();

builder.Services.AddHttpClient();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add database context
builder.Services.AddDbContext<Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Use global exception middleware
app.UseMiddleware<Api.Middleware.ExceptionMiddleware>();

// write line database connection string

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<Data.AppDbContext>();
        dbContext.Database.EnsureCreated();
        if (dbContext.Database.CanConnect())
        {
            app.Logger.LogInformation("Successfully connected to the database!");
            // Ensure database is created
            app.Logger.LogInformation("Database exists or has been created.");
        }
        else
        {
            app.Logger.LogError("Failed to connect to the database.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while testing database connection.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Use CORS before authorization
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
