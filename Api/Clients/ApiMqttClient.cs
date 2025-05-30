﻿using System.Text;
using Api.Services;
using Data;
using System.Text.Json;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace Api.Clients;

public class ApiMqttClient
{
      private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public ApiMqttClient(IConfiguration configuration)
    {
        var host = configuration["MQTT:Host"] ?? "localhost";
        var port = int.Parse(configuration["MQTT:Port"] ?? "1883");
        // var host = "4.208.73.116";
        
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"Api_{Guid.NewGuid()}");
        
        // if (configuration["MQTT:Username"] != null && configuration["MQTT:Password"] != null)
        // {
        //     builder.WithCredentials(configuration["MQTT:Username"], configuration["MQTT:Password"]);
        // }
        //
        Console.WriteLine("Trying to connect to host: " + host + " port: " + port);
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
    public async Task PublishMessage(string topic, string payload)
    {
        if (!_client.IsConnected)
        {
            Console.WriteLine("Client not connected, attempting to connect...");
            await ConnectAndKeepAlive();
        }

        var messageBuilder = new MqttApplicationMessageBuilder().WithTopic(topic);

        if (!string.IsNullOrEmpty(payload))
            messageBuilder.WithPayload(Encoding.UTF8.GetBytes(payload));
        
        var message = messageBuilder.Build();
        await _client.PublishAsync(message);
        Console.WriteLine($"Published message to topic '{topic}': {payload}");
    }
}