using System;
using System.Threading.Tasks;
using Api.Clients;
using Api.Services;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Tests
{
    public class ConfigurationServiceTests
    {
        private ConfigurationService CreateService()
        {
            // Provide dummy MQTT config
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { KeyValuePair.Create<string,string?>("MQTT:Host", "*" ) })
                .Build();
            var client = new ApiMqttClient(config);
            return new ConfigurationService(client);
        }

        [Fact]
        public async Task SendConfiguration_NullPreset_ThrowsNullReferenceException()
        {
            var svc = CreateService();
            var gh = new Greenhouse { Name="G", MacAddress="M", UserEmail="u@x", LightingMethod="L", WateringMethod="W", FertilizationMethod="F" };
            await Assert.ThrowsAsync<NullReferenceException>(() => svc.SendConfiguration(null!, gh));
        }

        [Fact]
        public async Task SendConfiguration_NullGreenhouse_ThrowsNullReferenceException()
        {
            var svc = CreateService();
            var preset = new Preset { Name="P1", MinTemperature=1, MaxTemperature=2, MinAirHumidity=3, MaxAirHumidity=4, MinSoilHumidity=5, MaxSoilHumidity=6, HoursOfLight=7 };
            await Assert.ThrowsAsync<NullReferenceException>(() => svc.SendConfiguration(preset, null!));
        }
    }
}
