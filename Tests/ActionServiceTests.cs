using Api.Services;
using Data.Entities;
using Tests.Helpers;
using Action = Data.Entities.Action;
using Api.Clients;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests
{
    public class ActionServiceTests
    {
        [Fact]
        public async Task GetActionsForPeriodAsync_ReturnsCorrectActions()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();

            var preset = new Preset
            {
                Name = "Default Preset",
                MinTemperature = 15,
                MaxTemperature = 30
            };

            var greenhouse = new Greenhouse
            {
                Name = "GH1",
                MacAddress = "192.168.0.10",
                LightingMethod = "LED",
                WateringMethod = "Auto",
                FertilizationMethod = "Auto",
                UserEmail = "test@example.com",
                ActivePreset = preset
            };

            dbContext.Greenhouses.Add(greenhouse);
            await dbContext.SaveChangesAsync();

            var greenhouseId = greenhouse.Id;
            var now = DateTime.UtcNow;

            dbContext.Actions.AddRange(
                new Action
                {
                    Type = "Irrigation",
                    Status = true,
                    Timestamp = now.AddMinutes(-30),
                    GreenhouseId = greenhouseId
                },
                new Action
                {
                    Type = "Ventilation",
                    Status = false,
                    Timestamp = now.AddHours(-1),
                    GreenhouseId = greenhouseId
                },
                new Action
                {
                    Type = "Lighting",
                    Status = true,
                    Timestamp = now.AddDays(-2),
                    GreenhouseId = greenhouseId
                }
            );

            await dbContext.SaveChangesAsync();

            // Create a test MQTT client
            var testMqttClient = new TestApiMqttClient();
            
            var service = new ActionService(dbContext, testMqttClient);

            // Act
            var result = await service.PrepareActionsForPeriodAsync(
                greenhouseId,
                now.AddHours(-2),
                now
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var types = result.Select(r => r.Type).ToList();
            Assert.Contains("Irrigation", types);
            Assert.Contains("Ventilation", types);
            Assert.DoesNotContain("Lighting", types);
        }

        [Fact]
        public async Task TriggerAction_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var testMqttClient = new TestApiMqttClient();
            var service = new ActionService(dbContext, testMqttClient);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.TriggerAction("nouser@example.com", 1, "TestType"));
        }

        [Fact]
        public async Task TriggerAction_GreenhouseNotLinked_ThrowsUnauthorizedAccessException()
        {
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "user@example.com", Password = "p" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            var testMqttClient = new TestApiMqttClient();
            var service = new ActionService(dbContext, testMqttClient);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.TriggerAction(user.email, 999, "TestType"));
        }

        [Fact]
        public async Task TriggerAction_Success_ThrowsMqttClientNotConnectedException()
        {
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "user@example.com", Password = "p" };
            dbContext.Users.Add(user);
            var greenhouse = new Greenhouse
            {
                Name = "GH",
                MacAddress = "MAC1",
                LightingMethod = "L",
                WateringMethod = "W",
                FertilizationMethod = "F",
                UserEmail = user.email
            };
            dbContext.Greenhouses.Add(greenhouse);
            await dbContext.SaveChangesAsync();
            var testMqttClient = new TestApiMqttClient();
            var service = new ActionService(dbContext, testMqttClient);
            await Assert.ThrowsAsync<MQTTnet.Exceptions.MqttClientNotConnectedException>(
                () => service.TriggerAction(user.email, greenhouse.Id, "Irrigation"));
        }
    }
}