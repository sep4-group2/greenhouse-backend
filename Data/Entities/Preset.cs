
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Database.Entities;
public class Preset
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public double MinAirHumidity { get; set; }
    public double MaxAirHumidity { get; set; }
    public double MinSoilHumidity { get; set; }
    public double MaxSoilHumidity { get; set; }
    public int HoursOfLight { get; set; }

    public ICollection<Greenhouse> Greenhouses { get; set; }

    public SystemPreset SystemPreset { get; set; }
    public UserPreset UserPreset { get; set; }
}