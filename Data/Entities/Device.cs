using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Device
{
    [Key]
    public int DeviceId { get; set; } 
    [Required]
    public string Endpoint { get; set; }
    [Required]
    public string P256dh { get; set; }
    [Required]
    public string Auth { get; set; }
    
    
    [Required]
    public string UserEmail { get; set; }
    [ForeignKey("UserEmail")]
    public User User { get; set; }
}