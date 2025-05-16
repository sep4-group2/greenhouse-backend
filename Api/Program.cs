using Api.Services;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DataService>();
// Add database context
builder.Services.AddDbContext<Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// write line database connection string
Console.WriteLine($"Database connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");

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
            app.Logger.LogInformation("Database exists or has been created.");
        }
        else
        {
            // log error and what happened
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

app.UseAuthorization();

app.MapControllers();

app.Run();