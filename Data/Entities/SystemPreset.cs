using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

public class SystemPreset
{
    [Key, ForeignKey("Preset")]
    public int Id { get; set; }

    public Preset Preset { get; set; }
}