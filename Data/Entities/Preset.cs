
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class Preset
{
    [Key]
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

    public ICollection<Greenhouse> Greenhouses { get; set; }

    public SystemPreset SystemPreset { get; set; }
    public UserPreset UserPreset { get; set; }
    public int? SystemPresetId { get; set; }
    public int? UserPresetId { get; set; }
}