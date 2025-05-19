using System.Text.Json;
using Api.Clients;
using Data;
using Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ActionService(AppDbContext context, MqttClient mqttClient)
{
    public async Task<List<ActionResultDTO>> PrepareActionsForPeriodAsync(int greenhouseId, DateTime start, DateTime end)
    {
        var actions = await context.Actions
            .Where(a => a.GreenhouseId == greenhouseId &&
                        a.Timestamp >= start &&
                        a.Timestamp <= end)
            .Select(a => new ActionResultDTO
            {
                Type = a.Type,
                Status = a.Status,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return actions;
    }

    public async Task TriggerAction(string userEmail, int greenhouseId, string actionType)
    {
        var user = await context.Users.Include(g => g.Greenhouses).FirstOrDefaultAsync(u => u.email == userEmail);
        if(user == null)
            throw new UnauthorizedAccessException("User not found");
        var greenhouse = user.Greenhouses.FirstOrDefault(g => g.Id == greenhouseId);
        if(greenhouse == null)
            throw new UnauthorizedAccessException("Greenhouse not found or not linked with this user");

        var mqttPayload = new
        {
            macAddress = greenhouse.IpAddress,
            action = actionType
        };
        var payloadJson = JsonSerializer.Serialize(mqttPayload);

        await mqttClient.PublishMessage(
            topic: $"greenhouse/{greenhouse.IpAddress}/{actionType}",
            payload: payloadJson
        );
    }
}