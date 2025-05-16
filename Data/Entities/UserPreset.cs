using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

public class UserPreset
{
    [Key, ForeignKey("Preset")]
    public int Id { get; set; }

    [Required]
    public string UserEmail { get; set; }

    [ForeignKey("UserEmail")]
    public User User { get; set; }

    public Preset Preset { get; set; }
}