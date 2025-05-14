using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Greenhouse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IpAddress { get; set; }
    public string LightingMethod { get; set; }
    public string WateringMethod { get; set; }
    public string FertilizationMethod { get; set; }

    [Required]
    public string UserEmail { get; set; }
    
    [ForeignKey("UserEmail")]
    public User User { get; set; }

    public int? ActivePresetId { get; set; }
    [ForeignKey("ActivePresetId")]
    public Preset ActivePreset { get; set; }

    public ICollection<SensorReading> SensorReadings { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    
    public ICollection<Action> Actions { get; set; }
}