using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<ActionService>();
// Add database context
builder.Services.AddDbContext<Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<Data.AppDbContext>();
        if (dbContext.Database.CanConnect())
        {
            app.Logger.LogInformation("Successfully connected to the database!");
            // Ensure database is created
            dbContext.Database.EnsureCreated();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("LocalDocker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();