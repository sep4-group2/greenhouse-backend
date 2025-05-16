using Api.Services;
using Data.Entities;
using Data.Utils;
using Tests.Helpers;

namespace Tests
{
    public class SensorReadingsServiceTests
    {
        [Fact]
        public async Task PrepareCurrentSensorReadingsAsync_ReturnsLatestSensorReadingsWithPresetBounds()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();

            var preset = new Preset
            {
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
            await dbContext.SaveChangesAsync();
            var greenhouseId = greenhouse.Id;
            
            dbContext.SensorReadings.AddRange(new[]
            {
                new SensorReading
                {
                    Type = SensorReadingType.Temperature,
                    Value = 32,
                    Unit = "°C",
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                    GreenhouseId = greenhouseId
                },
                new SensorReading
                {
                    Type = SensorReadingType.AirHumidity,
                    Value = 55,
                    Unit = "%",
                    Timestamp = DateTime.UtcNow.AddMinutes(-2),
                    GreenhouseId = greenhouseId
                },
                new SensorReading
                {
                    Type = SensorReadingType.SoilHumidity,
                    Value = 42,
                    Unit = "%",
                    Timestamp = DateTime.UtcNow.AddMinutes(-3),
                    GreenhouseId = greenhouseId
                }
            });

            await dbContext.SaveChangesAsync();

            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PrepareCurrentSensorReadingsAsync(greenhouse.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            var temp = result.First(r => r.Type == SensorReadingType.Temperature);
            Assert.Equal(32, temp.Value);
            Assert.Equal(10, temp.MinValue);
            Assert.Equal(30, temp.MaxValue);

            var air = result.First(r => r.Type == SensorReadingType.AirHumidity);
            Assert.Equal(55, air.Value);
            Assert.Equal(20, air.MinValue);
            Assert.Equal(70, air.MaxValue);

            var soil = result.First(r => r.Type == SensorReadingType.SoilHumidity);
            Assert.Equal(42, soil.Value);
            Assert.Equal(30, soil.MinValue);
            Assert.Equal(80, soil.MaxValue);
        }
    }
}