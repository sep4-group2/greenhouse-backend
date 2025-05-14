namespace Data.Database.Utils;

public class ReadingType
{
    public enum Type
    {
        Temperature,
        AirHumidity,
        SoilHumidity
    }

    public static string GetStringValue(Type type)
    {
        switch (type)
        {
            case Type.Temperature:
                return "temperature";
            case Type.AirHumidity:
                return "air humidity";
            case Type.SoilHumidity:
                return "soil humidity"; 
            default: 
                return "ERROR";
        }
    }
}
