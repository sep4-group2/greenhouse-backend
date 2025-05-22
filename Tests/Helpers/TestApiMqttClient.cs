using Api.Clients;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tests.Helpers;

public class TestApiMqttClient : ApiMqttClient
{
    public TestApiMqttClient() : base(CreateTestConfiguration())
    {
        // Initialize properties to avoid null issues
        LastTopic = string.Empty;
        LastPayload = string.Empty;
    }

    private static IConfiguration CreateTestConfiguration()
    {
        // Use KeyValuePair instead of Dictionary directly
        var configData = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("MQTT:Host", "localhost"),
            new KeyValuePair<string, string?>("MQTT:Port", "1883")
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        
        return configuration;
    }
    
    // Override the method to avoid actual MQTT communication in tests
    public new Task PublishMessage(string topic, string payload)
    {
        // Track the message for test assertions if needed
        LastTopic = topic;
        LastPayload = payload;
        
        // Do nothing in tests
        return Task.CompletedTask;
    }
    
    // Properties to track the last published message
    public string LastTopic { get; private set; }
    public string LastPayload { get; private set; }
}
