using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class CreatePresetDTO
{
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
    [Required]
    public double MinSoilHumidity { get; set; }
    [Required]
    public double MaxSoilHumidity { get; set; }
    [Required]
    public int HoursOfLight { get; set; }
    
}