using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DataConsumer.Services;
using Data;
using Data.Entities;
using Data.Utils;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebPushImpl.Services;
using WebPushImpl.DTOs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/*
 * Push Notification Integration Test
 * 
 * This test verifies that push notifications are sent when sensor readings go out of range.
 * 
 * To test with REAL push notifications:
 * 1. Generate real VAPID keys: npx web-push generate-vapid-keys
 * 2. Replace the VAPID keys in CreateTestConfiguration() with your real keys
 * 3. Open a browser and navigate to your notification API endpoint to get a real subscription
 * 4. Replace the Device endpoint, P256dh, and Auth values with real subscription data
 * 5. Run the test - you should receive an actual push notification!
 * 
 * Current behavior:
 * - Uses dummy VAPID keys and endpoints
 * - Push will fail (expected) but notification is still saved to database
 * - Test verifies the validation logic and database operations work correctly
 */

namespace Tests
{
    /// <summary>
    /// Testable notification service wrapper that allows us to verify push notification requests
    /// </summary>
    public class TestableNotificationService : INotificationService
    {
        private readonly NotificationService _innerService;
        public List<SendNotificationDTO> SentNotifications { get; } = new List<SendNotificationDTO>();
        
        public TestableNotificationService(IConfiguration configuration, AppDbContext dbContext, ILogger<NotificationService> logger)
        {
            _innerService = new NotificationService(configuration, dbContext, logger);
        }
        
        public async Task<VAPIDKeyDTO> GetPublicKey()
        {
            return await _innerService.GetPublicKey();
        }
        
        public async Task SaveSubscription(SaveSubscriptionDTO subscription)
        {
            await _innerService.SaveSubscription(subscription);
        }
        
        public async Task SendNotification(SendNotificationDTO notification)
        {
            // Track the notification request
            SentNotifications.Add(notification);
            
            // Call the real service
            await _innerService.SendNotification(notification);
        }
        
        public async Task SaveNotification(SaveNotificationDTO notification)
        {
            await _innerService.SaveNotification(notification);
        }
        
        public async Task<List<NotificationResultDTO>> GetNotificationsForPeriodAsync(int greenhouseId, DateTime start, DateTime end)
        {
            return await _innerService.GetNotificationsForPeriodAsync(greenhouseId, start, end);
        }
    }

    [Collection("IntegrationTests")]
    public class PushNotificationIntegrationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _notificationLogger;
        private readonly ILogger<SensorReadingValidator> _validatorLogger;

        public PushNotificationIntegrationTests()
        {
            _dbContext = TestDbHelper.GetInMemoryDbContext();
            _configuration = CreateTestConfiguration();
            _notificationLogger = NullLogger<NotificationService>.Instance;
            _validatorLogger = NullLogger<SensorReadingValidator>.Instance;
        }

        private static IConfiguration CreateTestConfiguration()
        {
            // To test with real push notifications, replace these with actual VAPID keys
            // You can generate VAPID keys using: npx web-push generate-vapid-keys
            // For testing without real notifications, these dummy keys will cause the push to fail gracefully
            var configData = new Dictionary<string, string?>
            {
                ["VAPIDKeys:PublicKey"] = "BEl62iUYgUivyIFSujosIVwdGnSzVgHRRvD_CZTJ2SXzSkqNBdWXtQ_-6TcWzNNBfZHhp6RIhxpOZPBfQqXFN7E",
                ["VAPIDKeys:PrivateKey"] = "s6LWTKyHqMHrxwJiEcZdJLHV0j1FYGUx-nkXS8h9UQ8"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task ValidateAndTriggerAsync_ShouldSendRealPushNotification_WhenOutOfRange()
        {
            // Arrange - Create test data
            var user = new User
            {
                email = "test@example.com",
                Password = "hashedpassword"
            };

            var preset = new Preset
            {
                Name = "Test Preset",
                MinTemperature = 10,
                MaxTemperature = 25,
                MinAirHumidity = 30,
                MaxAirHumidity = 70,
                MinSoilHumidity = 20,
                MaxSoilHumidity = 80
            };

            var greenhouse = new Greenhouse
            {
                Name = "Test Greenhouse",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                LightingMethod = "auto",
                WateringMethod = "auto",
                FertilizationMethod = "manual",
                UserEmail = user.email,
                ActivePreset = preset
            };

            // Add a mock device subscription for the user
            // In a real test, you would use actual push service endpoints
            var device = new Device
            {
                UserEmail = user.email,
                Endpoint = "https://fcm.googleapis.com/fcm/send/test-endpoint-12345",
                P256dh = "BNbXXcoReVUGC5Wn3S6mZnNr8G2C3xK5wF7rLttPYHp8LzTqtJHhF8fG3H2wYrpQst8F5Q6XYSn9Q2mZzN5G7X",
                Auth = "FP3C5fKdlrLK5H9GpCTIyQ"
            };

            _dbContext.Users.Add(user);
            _dbContext.Presets.Add(preset);
            _dbContext.Greenhouses.Add(greenhouse);
            _dbContext.Devices.Add(device);
            await _dbContext.SaveChangesAsync();

            var reading = new SensorReading
            {
                Type = SensorReadingType.Temperature,
                Value = 30, // Out of range (max is 25)
                Unit = "°C",
                Timestamp = DateTime.UtcNow,
                GreenhouseId = greenhouse.Id
            };

            // Create services with our testable notification service
            var testNotificationService = new TestableNotificationService(_configuration, _dbContext, _notificationLogger);
            var validator = new SensorReadingValidator(_dbContext, testNotificationService, _validatorLogger);

            // Act
            await validator.ValidateAndTriggerAsync(reading);

            // Assert - Check database notifications
            var notifications = await _dbContext.Notifications.ToListAsync();
            Assert.Single(notifications);
            Assert.Contains("temperature reading of 30 is out of range", notifications[0].Content);
            
            // Verify that a push notification request was actually sent
            Assert.Single(testNotificationService.SentNotifications);
            var sentNotification = testNotificationService.SentNotifications[0];
            Assert.Equal("test@example.com", sentNotification.userEmail);
            Assert.Equal(greenhouse.Id, sentNotification.GreenhouseId);
            Assert.Contains("temperature", sentNotification.notification.message.ToLower());
            Assert.NotNull(sentNotification.notification.title);

            // Note: This test will attempt to send a real push notification
            // The push will fail with mock endpoints, but that's expected for testing
            // To test with real notifications:
            // 1. Replace VAPID keys with real ones (generate with: npx web-push generate-vapid-keys)
            // 2. Replace device endpoint with a real subscription from a browser
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
