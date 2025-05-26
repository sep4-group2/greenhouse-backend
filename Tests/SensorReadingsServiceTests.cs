using Api.Services;
using Data.Entities;
using Data.Utils;
using Tests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Api.DTOs;

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
                MacAddress = "192.168.0.1",
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

        [Fact]
        public async Task PrepareCurrentSensorReadingsAsync_NoPreset_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PrepareCurrentSensorReadingsAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task PreparePastSensorReadingsAsync_NoRequest_ReturnsAllReadings()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var ghId = 1;
            var now = DateTime.UtcNow;
            dbContext.SensorReadings.AddRange(new[]
            {
                new SensorReading { Type = SensorReadingType.Temperature, Value = 10, Unit = "C", Timestamp = now.AddHours(-2), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.AirHumidity, Value = 20, Unit = "%", Timestamp = now.AddHours(-1), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.SoilHumidity, Value = 30, Unit = "%", Timestamp = now, GreenhouseId = ghId }
            });
            await dbContext.SaveChangesAsync();
            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PreparePastSensorReadingsAsync(ghId, null);

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task PreparePastSensorReadingsAsync_BeforeDate_FiltersCorrectly()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var ghId = 1;
            var now = DateTime.UtcNow;
            dbContext.SensorReadings.AddRange(
                new SensorReading { Type = SensorReadingType.Temperature, Value = 1, Unit = "C", Timestamp = now.AddDays(-2), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.Temperature, Value = 2, Unit = "C", Timestamp = now.AddDays(-1), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.Temperature, Value = 3, Unit = "C", Timestamp = now, GreenhouseId = ghId }
            );
            await dbContext.SaveChangesAsync();
            var request = new PastSensorReadingRequestDTO { BeforeDate = now.AddDays(-1) };
            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PreparePastSensorReadingsAsync(ghId, request);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.Timestamp <= request.BeforeDate.Value));
        }

        [Fact]
        public async Task PreparePastSensorReadingsAsync_AfterDate_FiltersCorrectly()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var ghId = 1;
            var now = DateTime.UtcNow;
            dbContext.SensorReadings.AddRange(
                new SensorReading { Type = SensorReadingType.Temperature, Value = 1, Unit = "C", Timestamp = now.AddDays(-2), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.Temperature, Value = 2, Unit = "C", Timestamp = now.AddDays(-1), GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.Temperature, Value = 3, Unit = "C", Timestamp = now, GreenhouseId = ghId }
            );
            await dbContext.SaveChangesAsync();
            var request = new PastSensorReadingRequestDTO { AfterDate = now.AddDays(-1) };
            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PreparePastSensorReadingsAsync(ghId, request);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.Timestamp >= request.AfterDate.Value));
        }

        [Fact]
        public async Task PreparePastSensorReadingsAsync_Type_FiltersCorrectly()
        {
            // Arrange
            var dbContext = TestDbHelper.GetInMemoryDbContext();
            var ghId = 1;
            var now = DateTime.UtcNow;
            dbContext.SensorReadings.AddRange(
                new SensorReading { Type = SensorReadingType.Temperature, Value = 5, Unit = "C", Timestamp = now, GreenhouseId = ghId },
                new SensorReading { Type = SensorReadingType.AirHumidity, Value = 15, Unit = "%", Timestamp = now, GreenhouseId = ghId }
            );
            await dbContext.SaveChangesAsync();
            var request = new PastSensorReadingRequestDTO { ReadingType = SensorReadingType.AirHumidity };
            var service = new SensorReadingsService(dbContext);

            // Act
            var result = await service.PreparePastSensorReadingsAsync(ghId, request);

            // Assert
            Assert.Single(result);
            Assert.Equal(SensorReadingType.AirHumidity, result.First().Type);
        }
    }
}