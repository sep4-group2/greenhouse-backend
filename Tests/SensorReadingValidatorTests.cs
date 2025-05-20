using System;
using System.Threading.Tasks;
using DataConsumer.Services;
using Data;
using Data.Entities;
using Data.Utils;
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
                MinTemperature = 10,
                MaxTemperature = 25
            };

            var greenhouse = new Greenhouse
            {
                Name = "Test Greenhouse",
                MacAddress = "192.168.1.10",
                LightingMethod = "bulb",
                WateringMethod = "auto",
                FertilizationMethod = "auto",
                UserEmail = "test@test.com",
                ActivePreset = preset
            };

            dbContext.Presets.Add(preset);
            dbContext.Greenhouses.Add(greenhouse);
            await dbContext.SaveChangesAsync();
            var greenhouseId = greenhouse.Id;

            var reading = new SensorReading
            {
                Type = SensorReadingType.Temperature,
                Value = 30, // out of range
                GreenhouseId = greenhouseId
            };

            var validator = new SensorReadingValidator(dbContext);

            // Act
            await validator.ValidateAndTriggerAsync(reading);
            
            // Assert
            var notifications = await dbContext.Notifications.ToListAsync();
            Assert.Single(notifications);
            Assert.Equal(2, notifications[0].GreenhouseId); // Because one is already seeded at this point 
        }
    }
}