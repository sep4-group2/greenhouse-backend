using Data.Database.Utils;

namespace Api.DTOs;

public class PastDataRequestDTO
{
    public DateTime? BeforeDate {get; set;}
    private DateTime? _afterDate;

    public DateTime? AfterDate
    {
        get
        {
            return _afterDate;
        }
        set
        {
            DateTime temp = DateTime.Now.AddMonths(12);
            if (value != null)
            {
                temp = value.Value;
            }
            if (DateTime.Compare(temp, DateTime.Now) < 0)
            {
                _afterDate = value;
            }
        }
    }

    private string? _readingType;

    public string? ReadingType
    {
        get
        {
            return _readingType;
        }
        set
        {
            if (value == SensorReadingType.Temperature || value == SensorReadingType.AirHumidity ||
                value == SensorReadingType.SoilHumidity)
            {
                _readingType = value;
            }
        }
    }
}