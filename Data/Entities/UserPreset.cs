using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Data.Entities;

public class UserPreset
{
    [Key, ForeignKey("Preset")]
    public int Id { get; set; }

    [Required]
    public string UserEmail { get; set; }

    [ForeignKey("UserEmail")]
    public User User { get; set; }

    [JsonIgnore]
    public Preset Preset { get; set; }
}