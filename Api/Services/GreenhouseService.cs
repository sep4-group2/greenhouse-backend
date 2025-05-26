using Api.DTOs;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Api.Services;

public class GreenhouseService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly string _mlBaseUrl;
    private readonly ConfigurationService _configurationService;

    public GreenhouseService(
        AppDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ConfigurationService configurationService
    )
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _httpClient = httpClientFactory.CreateClient();
        _mlBaseUrl = configuration["MalApi:BaseUrl"]
                     ?? throw new ArgumentNullException("MalApi:BaseUrl configuration is missing");
        _configurationService = configurationService;
    }

    public async Task<List<Greenhouse>> GetAllGreenhousesForUser(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        return await _dbContext.Greenhouses
            .Where(g => g.UserEmail == email)
            .Include(g => g.ActivePreset)
            .ToListAsync();
    }

    public async Task<Greenhouse> SetPresetAsync(Greenhouse greenhouse, Preset preset)
    {
        ArgumentNullException.ThrowIfNull(greenhouse);
        ArgumentNullException.ThrowIfNull(preset);

        _dbContext.Greenhouses.Update(greenhouse);
        await _dbContext.SaveChangesAsync();

        greenhouse.ActivePreset = preset;
        return greenhouse;
    }

    public async Task PairGreenhouse(GreenhousePairDto greenhousedto, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        var greenhouse = await _dbContext.Greenhouses.FirstOrDefaultAsync(g => g.MacAddress == greenhousedto.MacAddress
        );

        if (greenhouse == null)
        {
            greenhouse = new Greenhouse
            {
                Name = greenhousedto.Name,
                MacAddress = greenhousedto.MacAddress,
                UserEmail = email,
                LightingMethod = "manual",
                WateringMethod = "manual",
                FertilizationMethod = "manual",
            };
        }
        else if (greenhouse.UserEmail != null)
        {
            throw new UnauthorizedAccessException("This Greenhouse has already paired");
        }
        else
        {
            greenhouse.UserEmail = email;
        }

        _dbContext.Greenhouses.Add(greenhouse);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UnpairGreenhouse(int id, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");

        var greenhouse = await _dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == id);
        if (greenhouse == null)
        {
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        }

        greenhouse.UserEmail = null;
        greenhouse.User = null;

        _dbContext.Greenhouses.Update(greenhouse);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RenameGreenhouse(GreenhouseRenameDto greenhousedto, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");

        var greenhouse = await _dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhousedto.Id);
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");

        greenhouse.Name = greenhousedto.Name;

        _dbContext.Greenhouses.Update(greenhouse);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<PredictionResultDto> GetPredictionFromLatestValuesAsync(int greenhouseId)
    {
        // Fetch the latest sensor readings for each type
        var soilMoisture = await _dbContext.SensorReadings
            .Where(sr => sr.GreenhouseId == greenhouseId && sr.Type == "SoilHumidity")
            .Select(sr => sr.Value)
            .FirstOrDefaultAsync();

        var ambientTemperature = await _dbContext.SensorReadings
            .Where(sr => sr.GreenhouseId == greenhouseId && sr.Type == "Temperature")
            .Select(sr => sr.Value)
            .FirstOrDefaultAsync();

        var humidity = await _dbContext.SensorReadings
            .Where(sr => sr.GreenhouseId == greenhouseId && sr.Type == "Humidity")
            .Select(sr => sr.Value)
            .FirstOrDefaultAsync();

        if (soilMoisture == default || ambientTemperature == default || humidity == default)
        {
            throw new Exception("Missing sensor readings for the specified greenhouse");
        }

        // Create the prediction request DTO using the latest sensor readings
        var request = new PredictionRequestDto
        {
            Soil_Moisture = soilMoisture,
            Ambient_Temperature = ambientTemperature,
            Humidity = humidity
        };

        // Call the ML API with the constructed request
        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(_mlBaseUrl + "predict", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch prediction from ML service");
        }

        // Deserialize the response content directly into PredictionResultDto with case insensitivity
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<PredictionResultDto>(
            responseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (result == null)
        {
            throw new Exception("Failed to deserialize ML API response into PredictionResultDto");
        }

        return result;
    }

    public async Task SetPresetToGreenhouse(int greenhouseId, int presetId, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await _dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhouseId);
        Console.WriteLine($"Greenhouse: {greenhouse?.Name}, Email: {email}");
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Gresenhouse not found or not paired with this user");
        var preset = await _dbContext.Presets.FirstOrDefaultAsync(p => p.Id == presetId);
        if (preset == null)
            throw new UnauthorizedAccessException("Preset not found or not paired with this user");

        greenhouse.ActivePresetId = preset.Id;
        _dbContext.Greenhouses.Update(greenhouse);
        await _dbContext.SaveChangesAsync();
        await _configurationService.SendConfiguration(preset, greenhouse);
    }

    public async Task SetConfigurationForGreenhouse(string email, int greenhouseId, ConfigurationDto configuration)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await _dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhouseId);
        Console.WriteLine($"Greenhouse: {greenhouse?.Name}, Email: {email}");
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        switch (configuration.Type)
        {
            case "lighting":
                greenhouse.LightingMethod = configuration.Method;
                break;
            case "watering":
                greenhouse.WateringMethod = configuration.Method;
                break;
            case "fertilization":
                greenhouse.FertilizationMethod = configuration.Method;
                break;
            default:
                throw new UnauthorizedAccessException("Unknown configuration type");
        }

        var preset = await _dbContext.Presets.FirstOrDefaultAsync(p => p.Id == greenhouse.ActivePresetId);
        if (preset == null)
            throw new UnauthorizedAccessException("Preset not found or not paired with this user");

        _dbContext.Greenhouses.Update(greenhouse);
        await _dbContext.SaveChangesAsync();
        await _configurationService.SendConfiguration(preset, greenhouse);
    }
}