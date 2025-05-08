using System.Text;
using System.Text.Json;
using Data.Database;
using Data.Database.Entities;
using DataConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace DataConsumer.Clients;

public class SimpleMqttClient
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly AppDbContext _dbContext;
    private readonly SensorReadingValidator _validator;

    public SimpleMqttClient(IConfiguration configuration, AppDbContext dbContext)
    {
        var host = configuration["MQTT:Host"] ?? "localhost";
        var port = int.Parse(configuration["MQTT:Port"] ?? "1883");
        _dbContext = dbContext;
        _validator = new SensorReadingValidator(dbContext);
            
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
          
        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            Console.WriteLine($"Received message on topic '{topic}': {message}");
            
            await HandleMessage(topic, message);
        };

            
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"DataConsumer_{Guid.NewGuid()}")
            .Build();
    }

    public async Task ConnectAndKeepAlive()
    {
        try
        {
            var result = await _client.ConnectAsync(_options);
            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully!");
                    
                // Set up reconnection if connection is lost
                _client.DisconnectedAsync += async (e) =>
                {
                    Console.WriteLine("Disconnected from MQTT broker, reconnecting...");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _client.ConnectAsync(_options);
                    }
                    catch
                    {
                        Console.WriteLine("Reconnection failed");
                    }
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect: {result.ResultCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
    }
     
    public async Task SubscribeToTopics(string[] topics)
    {
        foreach (var topic in topics)
        {
            try
            {
                await _client.SubscribeAsync(topic);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to subscribe to topic: {topic} /n" + e.Message);
            }
            
        }
    }
    
    private async Task HandleMessage(string topic, string message)
    {
        if (topic == "greenhouse/sensor")
        {
            try
            {
                var sensorData = JsonSerializer.Deserialize<SensorReading>(message);
                var data = new SensorReading()
                {
                    Type = sensorData.Type,
                    Value = sensorData.Value,
                    Unit = sensorData.Unit,
                    Timestamp = sensorData.Timestamp,
                    GreenhouseId = sensorData.GreenhouseId,
                    Greenhouse = sensorData.Greenhouse
                };
                    //edit the data based on what is actually coming through
            
                _dbContext.SensorReadings.Add(data);
                await _dbContext.SaveChangesAsync();
                await _validator.ValidateAndTriggerAsync(data);
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Failed to parse message: {e.Message}");
            }}
        //insert notification action and save to database if message is action
    }
}