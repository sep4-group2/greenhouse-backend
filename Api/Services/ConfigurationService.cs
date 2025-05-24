using System.Text.Json;
using Api.Clients;
using Data;
using Data.Entities;

namespace Api.Services;

public class ConfigurationService( ApiMqttClient apiMqttClient)
{
    public async Task SendConfiguration(Preset preset, Greenhouse greenhouse)
    {
        var payload = new
        {
            WateringMethod = greenhouse.WateringMethod,
            FertilizationMethod = greenhouse.FertilizationMethod,
            LightingMethod = greenhouse.LightingMethod,
            HoursOfLight = preset.HoursOfLight,
            MinSoilHumidity = preset.MinSoilHumidity,
            MaxSoilHumidity = preset.MaxSoilHumidity,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        

        await apiMqttClient.PublishMessage(
            topic: $"greenhouse/{greenhouse.MacAddress}/Configuration",
            payload: JsonSerializer.Serialize(payload)
        );
    }
}