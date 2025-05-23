using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class UpdatePresetDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
    
    [Required]
    public double MinTemperature { get; set; }
    
    [Required]
    public double MaxTemperature { get; set; }
    
    [Required]
    public double MinAirHumidity { get; set; }
    
    [Required]
    public double MaxAirHumidity { get; set; }
    public double MinSoilHumidity { get; set; }
    public double MaxSoilHumidity { get; set; }
    public int HoursOfLight { get; set; }
}