using Api.Services;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tests.Helpers;
using Xunit;

public class NotificationServiceTests
{
    [Fact]
    public async Task GetNotificationsForPeriodAsync_ReturnsCorrectNotifications()
    {
        // Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();

        var greenhouse = new Greenhouse
        {
            Name = "GH1",
            IpAddress = "192.168.0.10",
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
        
        //Had to add some stuff because of the constructor
        var service = new NotificationService(new ConfigurationManager(), dbContext, new Logger<NotificationService>(new LoggerFactory()));

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