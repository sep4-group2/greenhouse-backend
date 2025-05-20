using System.Text.Json;
using Data;
using DataConsumer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataConsumer.Service;

public class ActionService (AppDbContext context)
{
    public async Task HandleAction(string message)
    {
        try
        {
            var actionMessageDto = JsonSerializer.Deserialize<ActionMessageDTO>(message);
            if (actionMessageDto == null)
            {
                throw new JsonException("Failed to deserialize action message.");
            }
            var greenhouse = await context.Greenhouses
                .FirstOrDefaultAsync(g => g.MacAddress == actionMessageDto.MacAddress);
            var action = new Data.Entities.Action
            {
                Type = actionMessageDto.Command,
                Status = actionMessageDto.Status.ToString(),
                Timestamp = actionMessageDto.Timestamp,
                GreenhouseId = greenhouse?.Id ?? 0
            };
            context.Actions.Add(action);
            await context.SaveChangesAsync();
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Failed to parse action message: {e.Message}");
        }
    }
}