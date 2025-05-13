using System;
using System.Threading.Tasks;
using DataConsumer.Services;
using Data.Database;
using Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;

namespace Tests
{
    public class SensorReadingValidatorTests
    {
        [Fact]
        public async Task ValidateAndTriggerAsync_ShouldLog_WhenTemperatureOutOfRange()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();

            var preset = new Preset
            {
                Name = "Test",
                Id = 1,
                MinTemperature = 10,
                MaxTemperature = 25
            };

            var greenhouse = new Greenhouse
            {
                Id = 1,
                Name = "Test Greenhouse",
                IpAddress = "192.168.1.10",
                LightingMethod = "bulb",
                WateringMethod = "auto",
                FertilizationMethod = "auto",
                UserEmail = "test@test.com",
                ActivePreset = preset
            };

            dbContext.Presets.Add(preset);
            dbContext.Greenhouses.Add(greenhouse);
            await dbContext.SaveChangesAsync();

            var reading = new SensorReading
            {
                Type = "temperature",
                Value = 30, // out of range
                GreenhouseId = 1
            };

            var validator = new SensorReadingValidator(dbContext);

            // Act
            await validator.ValidateAndTriggerAsync(reading);
            
            // Assert
            var notifications = await dbContext.Notifications.ToListAsync();
            Assert.Single(notifications);
            Assert.Contains("temperature reading of 30 is out of range.", notifications[0].Content);
            Assert.Equal(1, notifications[0].GreenhouseId);
        }
    }
}