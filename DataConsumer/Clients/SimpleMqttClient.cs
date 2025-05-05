using Microsoft.Extensions.Configuration;
using MQTTnet;

namespace DataConsumer.Clients;

public class SimpleMqttClient
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public SimpleMqttClient(IConfiguration configuration)
    {
        var host = configuration["MQTT:Host"] ?? "localhost";
        var port = int.Parse(configuration["MQTT:Port"] ?? "1883");
            
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
            
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
}