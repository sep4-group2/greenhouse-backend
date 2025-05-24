using System.Text.Json;
using Data;
using DataConsumer.DTOs;
using DataConsumer.Services;
using Microsoft.EntityFrameworkCore;

namespace DataConsumer.Service;

public class SensorService(AppDbContext dbContext, SensorReadingValidator validator)
{
    public async Task HandleSensorData(string message)
    {
        try
        {
            var sensorDataDto = JsonSerializer.Deserialize<SensorReadingMessageDto>(message);

            if (sensorDataDto == null)
            {
                throw new JsonException("Failed to deserialize sensor data message.");
            }

            var greenhouse = await dbContext.Greenhouses
                .FirstAsync(g => g.MacAddress == sensorDataDto.MacAddress);

            foreach (var sensorData in sensorDataDto.SensorData)
            {
                var data = new SensorReading
                {
                    Type = sensorData.Type,
                    Value = sensorData.Value,
                    Unit = sensorData.Unit,
                    Timestamp = DateTime.Now,
                    GreenhouseId = greenhouse.Id
                };
                dbContext.SensorReadings.Add(data);
                await dbContext.SaveChangesAsync();
                await validator.ValidateAndTriggerAsync(data);
            }
            
            Console.WriteLine("Sensor data validated");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Failed to parse message: {e.Message}");
        } 
        catch (DbUpdateException e)
        {
            Console.WriteLine($"Failed to save sensor data: {e.Message}");
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