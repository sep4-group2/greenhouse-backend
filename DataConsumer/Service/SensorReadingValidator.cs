using Data;
using Data.Entities;
using Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace DataConsumer.Services;

public class SensorReadingValidator
{
    private readonly AppDbContext _dbContext;

    public SensorReadingValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ValidateAndTriggerAsync(SensorReading reading)
    {
        var greenhouse = await _dbContext.Greenhouses
            .Include(g => g.ActivePreset)
            .FirstOrDefaultAsync(g => g.Id == reading.GreenhouseId);

        if (greenhouse?.ActivePreset == null)
        {
            Console.WriteLine("No preset assigned. Skipping validation.");
            return;
        }

        var preset = greenhouse.ActivePreset;
        var type = reading.Type.ToLower();
        var outOfRange = OutOfRange(reading, type, preset);
        if (!outOfRange) return;

        Console.WriteLine($"Out-of-range reading detected: {reading.Type} = {reading.Value}");

        
        //TODO should be passed to notification server
        var notification = new Notification
        {
            Content = $"{reading.Type} reading of {reading.Value} is out of range.",
            Timestamp = DateTime.UtcNow,
            GreenhouseId = reading.GreenhouseId
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }

    private static bool OutOfRange(SensorReading reading, string type, Preset preset)
    {
        bool outOfRange = type switch
        {
            SensorReadingType.Temperature => reading.Value < preset.MinTemperature || reading.Value > preset.MaxTemperature,
            SensorReadingType.AirHumidity => reading.Value < preset.MinAirHumidity || reading.Value > preset.MaxAirHumidity,
            SensorReadingType.SoilHumidity => reading.Value < preset.MinSoilHumidity || reading.Value > preset.MaxSoilHumidity,
            _ => false
        };
        return outOfRange;
    }
}