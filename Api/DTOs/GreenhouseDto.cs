namespace Api.DTOs;

public class GreenhouseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MacAddress { get; set; }
    public string LightingMethod { get; set; }
    public string WateringMethod { get; set; }
    public string FertilizationMethod { get; set; }
    public string UserEmail { get; set; }
    public int? ActivePresetId { get; set; }
    public string? ActivePresetName { get; set; }
}