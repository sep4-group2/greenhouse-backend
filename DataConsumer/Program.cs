using Microsoft.Extensions.Configuration;
using DataConsumer;
using DataConsumer.Clients;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Data.Database;

// Set up configuration
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var topics = new[]
{
    "air humidity",
    "soil humidity",
    "CO2 levels",
    "temperature",
    "brightness"
};

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() // This allows overriding settings with environment variables
    .Build();

// Now you can access settings
var connectionString = configuration.GetConnectionString("DefaultConnection");
var mqttHost = configuration["MQTT:Host"];
var mqttPort = configuration["MQTT:Port"];

Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"appsettings.{environment}.json");
Console.WriteLine($"MQTT Host: {mqttHost}");
Console.WriteLine($"MQTT Port: {mqttPort}");
Console.WriteLine($"Connection String: {connectionString}");

try
{
    // Test database connection
    Console.WriteLine("Testing database connection...");
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    var dbContext = new AppDbContext(optionsBuilder.Options);
    using (dbContext)
    {
        // This will verify if we can connect to the database
        bool canConnect = dbContext.Database.CanConnect();
        
        if (canConnect)
        {
            Console.WriteLine("Successfully connected to the database!");
            
            // Ensure database is created (this is optional and creates the database if it doesn't exist)
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database exists or has been created.");
        }
        else
        {
            Console.WriteLine("Failed to connect to the database.");
        }
    }
    
    // Create and use the simple MQTT client
    var mqttClient = new SimpleMqttClient(configuration, dbContext);
    await mqttClient.ConnectAndKeepAlive();
    await mqttClient.SubscribeToTopics(topics);

    // Keep the application running without using Console.ReadKey()
    Console.WriteLine("Client connected and running. The application will continue running until terminated.");
    
    // Use a ManualResetEvent to keep the application running
    var waitHandle = new ManualResetEvent(false);
    
    // Handle SIGTERM and SIGINT for graceful shutdown
    AppDomain.CurrentDomain.ProcessExit += (s, e) => waitHandle.Set();
    Console.CancelKeyPress += (s, e) => {
        e.Cancel = true;
        waitHandle.Set();
    };
    
    // Wait indefinitely until signal
    waitHandle.WaitOne();
}
catch (Exception ex)
{
    Console.WriteLine($"Application error: {ex.Message}");
    // Exit with non-zero code to indicate error
    Environment.Exit(1);
}