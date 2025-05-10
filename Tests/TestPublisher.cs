using System.Text.Json;
using MQTTnet;

namespace Tests;

public static class TestPublisher
{
    public static async Task SendTestSensorReadingAsync()
    {
        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // adjust if needed
            .WithClientId("TestPublisher")
            .Build();

        try
        {
            await client.ConnectAsync(options);

            var testReading = new SensorReading
            {
                Id = 0, // typically auto-generated in DB
                Type = "temperature",
                Value = 24.6,
                Unit = "C",
                Timestamp = DateTime.UtcNow,
                GreenhouseId = 1,
                Greenhouse = null // can be omitted or set to null
            };

            string json = JsonSerializer.Serialize(testReading);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("greenhouse/sensor")
                .WithPayload(json)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            Console.WriteLine("Publishing test sensor message:");
            Console.WriteLine(json);

            await client.PublishAsync(message);

            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MQTT publish error: {ex.Message}");
        }
    }
}