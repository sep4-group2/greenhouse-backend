using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class User
{
    [Key]
    public string email { get; set; }
    [Required]
    public string Password { get; set; }

    public ICollection<Greenhouse> Greenhouses { get; set; }
    public ICollection<UserPreset> UserPresets { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Device> Devices { get; set; }
}