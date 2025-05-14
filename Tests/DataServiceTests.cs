using Api.Services;
using Data.Database.Entities;
using Tests.Helpers;

namespace Tests
{
    public class SensorDataServiceTests
    {
        [Fact]
        public async Task GetCurrentDataAsync_ReturnsLatestSensorReadingsWithPresetBounds()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();

            var preset = new Preset
            {
                Id = 1,
                Name = "Test Preset",
                MinTemperature = 10,
                MaxTemperature = 30,
                MinAirHumidity = 20,
                MaxAirHumidity = 70,
                MinSoilHumidity = 30,
                MaxSoilHumidity = 80
            };

            var greenhouse = new Greenhouse
            {
                Id = 1,
                Name = "GH Test",
                IpAddress = "192.168.0.1",
                LightingMethod = "LED",
                WateringMethod = "auto",
                FertilizationMethod = "manual",
                UserEmail = "test@test.com",
                ActivePreset = preset
            };

            dbContext.Presets.Add(preset);
            dbContext.Greenhouses.Add(greenhouse);
            dbContext.SensorReadings.AddRange(new[]
            {
                new SensorReading
                {
                    Type = "temperature",
                    Value = 32,
                    Unit = "°C",
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                    GreenhouseId = 1
                },
                new SensorReading
                {
                    Type = "air humidity",
                    Value = 55,
                    Unit = "%",
                    Timestamp = DateTime.UtcNow.AddMinutes(-2),
                    GreenhouseId = 1
                },
                new SensorReading
                {
                    Type = "soil humidity",
                    Value = 42,
                    Unit = "%",
                    Timestamp = DateTime.UtcNow.AddMinutes(-3),
                    GreenhouseId = 1
                }
            });

            await dbContext.SaveChangesAsync();

            var service = new DataService(dbContext);

            // Act
            var result = await service.GetCurrentDataAsync(greenhouse.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            var temp = result.First(r => r.Type == "temperature");
            Assert.Equal(32, temp.Value);
            Assert.Equal(10, temp.MinValue);
            Assert.Equal(30, temp.MaxValue);

            var air = result.First(r => r.Type == "air humidity");
            Assert.Equal(55, air.Value);
            Assert.Equal(20, air.MinValue);
            Assert.Equal(70, air.MaxValue);

            var soil = result.First(r => r.Type == "soil humidity");
            Assert.Equal(42, soil.Value);
            Assert.Equal(30, soil.MinValue);
            Assert.Equal(80, soil.MaxValue);
        }
    }
}