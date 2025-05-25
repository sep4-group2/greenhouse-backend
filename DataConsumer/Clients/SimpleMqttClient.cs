using System.Text;
using System.Text.Json;
using Data;
using Data.Entities;
using DataConsumer.Service;
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
    private readonly SensorService _sensorService;
    private readonly ActionService _actionService;

    public SimpleMqttClient(IConfiguration configuration, AppDbContext dbContext, SensorService sensorService, ActionService actionService)
    {
        var host = configuration["MQTT:Host"] ?? "localhost";
        var port = int.Parse(configuration["MQTT:Port"] ?? "1883");
        
        _dbContext = dbContext;
        _sensorService = sensorService;
        _actionService = actionService;
            
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
          
        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            Console.WriteLine($"Received message on topic '{topic}': {message}");
            
            await HandleMessage(topic, message);
        };


        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"DataConsumer_{Guid.NewGuid()}");
        
        if (configuration["MQTT:Username"] != null && configuration["MQTT:Password"] != null)
        {
            builder.WithCredentials(configuration["MQTT:Username"], configuration["MQTT:Password"]);
        }
        
        _options = builder
            .WithCleanSession()
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
        Console.WriteLine($"Handling message on topic '{topic}': {message}");
        if (topic == "greenhouse/sensor")
        {
            await _sensorService.HandleSensorData(message);
        }
        else if (topic == "greenhouse/action")
        {
            await _actionService.HandleAction(message);
        }

        //insert notification action and save to database if message is action
    }
}