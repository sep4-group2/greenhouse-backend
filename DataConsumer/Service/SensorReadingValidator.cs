using Data;
using Data.Entities;
using Data.Utils;
using Microsoft.EntityFrameworkCore;
using WebPushImpl.Services;
using WebPushImpl.DTOs;
using Microsoft.Extensions.Logging;

namespace DataConsumer.Services;

public class SensorReadingValidator
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SensorReadingValidator> _logger;

    public SensorReadingValidator(AppDbContext dbContext, INotificationService notificationService, ILogger<SensorReadingValidator> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
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

        // Send push notification to user
        try
        {
            var notificationMessage = $"{reading.Type} reading of {reading.Value} is out of range.";
            
            await _notificationService.SendNotification(new SendNotificationDTO
            {
                userEmail = greenhouse.UserEmail,
                GreenhouseId = reading.GreenhouseId,
                notification = new NotificationDTO
                {
                    title = $"Alert: {greenhouse.Name}",
                    message = notificationMessage
                }
            });
            
            _logger.LogInformation($"Push notification sent to {greenhouse.UserEmail} for greenhouse {greenhouse.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send push notification to {greenhouse.UserEmail}");
        }

        // Save notification to database as backup
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
            SensorReadingType.Temperature => reading.Value < preset.MinTemperature ||
                                             reading.Value > preset.MaxTemperature,
            SensorReadingType.AirHumidity => reading.Value < preset.MinAirHumidity ||
                                             reading.Value > preset.MaxAirHumidity,
            SensorReadingType.SoilHumidity => reading.Value < preset.MinSoilHumidity ||
                                              reading.Value > preset.MaxSoilHumidity,
            _ => false
        };
        return outOfRange;
    }
}