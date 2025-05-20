using System.Text.Json;
using Data;
using DataConsumer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataConsumer.Service;

public class ActionService(AppDbContext context)
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
            
            if (greenhouse == null)
            {
                Console.WriteLine($"Greenhouse with MAC address {actionMessageDto.MacAddress} not found.");
                return;
            }
            
            var action = new Data.Entities.Action
            {
                Type = actionMessageDto.Command,
                Status = actionMessageDto.Status,
                Timestamp = actionMessageDto.Timestamp,
                GreenhouseId = greenhouse.Id
            };
            context.Actions.Add(action);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Action {action.Type} with status {action.Status} for greenhouse {greenhouse.MacAddress} saved.");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Failed to parse action message: {e.Message}");
        } 
        catch (DbUpdateException e)
        {
            Console.WriteLine($"Failed to save action: {e.Message}");
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine($"Greenhouse not found: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
        }
    }
}