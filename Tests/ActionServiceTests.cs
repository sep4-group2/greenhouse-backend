using Api.Services;
using Data.Entities;
using Tests.Helpers;
using Action = Data.Entities.Action;

namespace Tests
{
    /*public class ActionServiceTests
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

            var service = new ActionService(dbContext);

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
    }*/
}