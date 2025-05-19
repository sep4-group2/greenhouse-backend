using System.Text.Json;
using Data;

namespace DataConsumer.Services;

public class SensorService(AppDbContext dbContext, SensorReadingValidator validator)
{
    public async Task HandleSensorData(string message)
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
            
            dbContext.SensorReadings.Add(data);
            await dbContext.SaveChangesAsync();
            await validator.ValidateAndTriggerAsync(data);
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Failed to parse message: {e.Message}");
        }
    }

}