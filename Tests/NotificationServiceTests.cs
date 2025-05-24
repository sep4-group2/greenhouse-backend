using Api.DTOs;
using Api.Services;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tests.Helpers;
using WebPush;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;

public class NotificationServiceTests
{
    [Fact]
    public async Task
        GetNotificationsForPeriodAsync_ReturnsCorrectNotifications()
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

        //Had to add some stuff because of the constructor
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

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

    [Fact]
    public async Task GetPublicKey_ReturnsNonNullValue()
    {
        //Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

        //Act
        var result = service.GetPublicKey();

        //Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveSubscription_SavesOneSubscription()
    {
        //Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

        var subscription = new SaveSubscriptionDTO()
        {
            Auth = "xYZ123AbCdEfGhIj",
            Endpoint =
                "https://fcm.googleapis.com/fcm/send/fake-endpoint-id-12345",
            P256dh =
                "BNc5W0HbVjKl1dZcRzd7XbEZHUXD7VzGaUlRAjrhL1vU-RgSzFzU2X5E5W-BJhZK7e57Go5_4ZFsTTN5vWKrP0Y",
            userEmail = "someEmail@gmail.com"
        };

        Assert.Empty(dbContext.Devices);

        //Act
        await service.SaveSubscription(subscription);

        //Assert
        Assert.Equal(1, dbContext.Devices.Count());
    }

    [Fact]
    public async Task SaveSubscription_SavesCorrectSubscription()
    {
        //Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

        var subscription = new SaveSubscriptionDTO()
        {
            Auth = "xYZ123AbCdEfGhIj",
            Endpoint =
                "https://fcm.googleapis.com/fcm/send/fake-endpoint-id-12345",
            P256dh =
                "BNc5W0HbVjKl1dZcRzd7XbEZHUXD7VzGaUlRAjrhL1vU-RgSzFzU2X5E5W-BJhZK7e57Go5_4ZFsTTN5vWKrP0Y",
            userEmail = "someEmail@gmail.com"
        };

        //Act
        await service.SaveSubscription(subscription);

        //Assert
        var savedDevice = dbContext.Devices.First();
        Assert.Equal("xYZ123AbCdEfGhIj", savedDevice.Auth);
        Assert.Equal(
            "https://fcm.googleapis.com/fcm/send/fake-endpoint-id-12345",
            savedDevice.Endpoint);
        Assert.Equal(
            "BNc5W0HbVjKl1dZcRzd7XbEZHUXD7VzGaUlRAjrhL1vU-RgSzFzU2X5E5W-BJhZK7e57Go5_4ZFsTTN5vWKrP0Y",
            savedDevice.P256dh);
        Assert.Equal("someEmail@gmail.com", savedDevice.UserEmail);
    }

    [Fact]
    public async Task SaveNotification_SavesOneNotification()
    {
        //Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

        var notification = new SaveNotificationDTO()
        {
            Content =
                "{ \"title\": \"Test notifications\", \"message\": \"Testing\" }",
            GreenhouseId = 1,
            Timestamp = DateTime.Now
        };
        
        Assert.Empty(dbContext.Notifications);
        
        //Act
        await service.SaveNotification(notification);
        
        //Assert
        Assert.Equal(1, dbContext.Notifications.Count());
    }

    [Fact]
    public async Task SaveNotification_SavesCorrectNotification()
    {
        //Arrange
        var dbContext = TestDbHelper.GetInMemoryDbContext();
        var service = new NotificationService(new ConfigurationManager(),
            dbContext, new Logger<NotificationService>(new LoggerFactory()));

        var notification = new SaveNotificationDTO()
        {
            Content =
                "{ \"title\": \"Test notifications\", \"message\": \"Testing\" }",
            GreenhouseId = 1,
            Timestamp = new DateTime(2025, 5, 21, 9, 25, 35)
        };
        
        //Act
        await service.SaveNotification(notification);
        
        //Assert
        var savedNotification = dbContext.Notifications.First();
        Assert.Equal("{ \"title\": \"Test notifications\", \"message\": \"Testing\" }", savedNotification.Content);
        Assert.Equal(1, savedNotification.GreenhouseId);
        Assert.Equal(new DateTime(2025, 5, 21, 9, 25, 35), savedNotification.Timestamp);
    }
}