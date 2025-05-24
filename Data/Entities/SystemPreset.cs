using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Data.Entities;

public class SystemPreset
{
    [Key, ForeignKey("Preset")]
    public int Id { get; set; }

    [JsonIgnore]
    public Preset Preset { get; set; }
}