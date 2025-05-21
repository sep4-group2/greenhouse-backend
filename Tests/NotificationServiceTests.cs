using Api.Services;
using Data.Entities;
using Tests.Helpers;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

public class NotificationServiceTests
{
    [Fact]
    public async Task GetNotificationsForPeriodAsync_ReturnsCorrectNotifications()
    {
        // Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        
        // Create configuration and logger for NotificationService
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var logger = NullLogger<NotificationService>.Instance;

        var greenhouse = new Greenhouse
        {
            Name = "GH1",
            MacAddress = "00:1A:2B:3C:4D:5E",
            LightingMethod = "LED",
            WateringMethod = "Auto",
            FertilizationMethod = "Auto",
            UserEmail = "test@test.com"
        };

        dbContext.Greenhouses.Add(greenhouse);
        await dbContext.SaveChangesAsync();

        var greenhouseId = greenhouse.Id;
        var now = DateTime.UtcNow;

        dbContext.Notifications.AddRange(
            new Notification
            {
                Timestamp = now.AddMinutes(-30),
                Content = "Notification 1",
                GreenhouseId = greenhouseId
            },
            new Notification
            {
                Timestamp = now.AddHours(-1),
                Content = "Notification 2",
                GreenhouseId = greenhouseId
            },
            new Notification
            {
                Timestamp = now.AddDays(-2),
                Content = "Notification 3",
                GreenhouseId = greenhouseId
            }
        );

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(configuration, dbContext, logger);

        // Act
        var result = await service.GetNotificationsForPeriodAsync(
            greenhouseId,
            now.AddHours(-2),
            now
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var contents = result.Select(r => r.Content).ToList();
        Assert.Contains("Notification 1", contents);
        Assert.Contains("Notification 2", contents);
        Assert.DoesNotContain("Notification 3", contents);
    }
}