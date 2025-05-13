using Data.Database.Entities;

namespace Data.Database.Services;

public class GreenhouseService 
{
    public Greenhouse SetPreset(Greenhouse greenhouse, Preset preset)
    {
        if (greenhouse == null)
            throw new ArgumentNullException(nameof(greenhouse));

        if (preset == null)
            throw new ArgumentNullException(nameof(preset));

        greenhouse.ActivePreset = preset;
        return greenhouse;
    }
}