namespace Tests;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        await TestPublisher.SendTestSensorReadingAsync();
    }
}