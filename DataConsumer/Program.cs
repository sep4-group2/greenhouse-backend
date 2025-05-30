﻿using Microsoft.Extensions.Configuration;
using DataConsumer;
using DataConsumer.Clients;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Data;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DataConsumer.Service;
using DataConsumer.Services;
using WebPushImpl.Services;
using Microsoft.Extensions.Logging;

// Set up configuration
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Release";
var topics = new[]
{
    "greenhouse/sensor",
    "greenhouse/action"
};


var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // This allows overriding settings with environment variables

// Only add KeyVault in Production/Release environment
if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase) ||
    environment.Equals("Release", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Running in Production/Release environment - configuring KeyVault...");

    // Build temporary configuration to get KeyVault settings
    var tempConfig = configBuilder.Build();

    // Get KeyVault settings from configuration
    var keyVaultName = tempConfig["KeyVault:Vault"];
    var managedIdentityClientId = tempConfig["KeyVault:ManagedIdentityClientId"];

    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

        // Add KeyVault to the configuration sources
        configBuilder.AddAzureKeyVault(keyVaultUri, credential);
        Console.WriteLine($"KeyVault configuration added: {keyVaultUri}");
    }
    else
    {
        Console.WriteLine("KeyVault name not found in configuration.");
    }
}
else
{
    Console.WriteLine($"Running in {environment} environment - KeyVault integration disabled.");
}

IConfiguration configuration = configBuilder.Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

try
{
    // Test database connection
    Console.WriteLine("Testing database connection...");
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseSqlServer(connectionString);
    var dbContext = new AppDbContext(optionsBuilder.Options);
        // This will verify if we can connect to the database
        bool canConnect = dbContext.Database.CanConnect();

        dbContext.Database.EnsureCreated();

        if (canConnect)
        {
            Console.WriteLine("Successfully connected to the database!");
        }
        else
        {
            Console.WriteLine("Failed to connect to the database.");
        }
    
    // Set up logging
    using var loggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole()
               .SetMinimumLevel(LogLevel.Information));
    
    var validatorLogger = loggerFactory.CreateLogger<SensorReadingValidator>();
    var notificationLogger = loggerFactory.CreateLogger<NotificationService>();
    
    // Create notification service
    var notificationService = new NotificationService(configuration, dbContext, notificationLogger);
    
    // Create services with dependencies
    var sensorReadingValidator = new SensorReadingValidator(dbContext, notificationService, validatorLogger);
    var sensorService = new SensorService(dbContext, sensorReadingValidator);
    var actionService = new ActionService(dbContext);
    // Create and use the simple MQTT client
    var mqttClient = new SimpleMqttClient(configuration, dbContext, sensorService, actionService);
    await mqttClient.ConnectAndKeepAlive();
    await mqttClient.SubscribeToTopics(topics);

    // Keep the application running without using Console.ReadKey()
    Console.WriteLine("Client connected and running. The application will continue running until terminated.");

    // Use a ManualResetEvent to keep the application running
    var waitHandle = new ManualResetEvent(false);

    // Handle SIGTERM and SIGINT for graceful shutdown
    AppDomain.CurrentDomain.ProcessExit += (s, e) => waitHandle.Set();
    Console.CancelKeyPress += (s, e) =>
    {
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