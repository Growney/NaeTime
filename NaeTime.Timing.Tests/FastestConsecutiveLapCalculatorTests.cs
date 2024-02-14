using NaeTime.Timing.Models;

namespace NaeTime.Timing.Tests;
public class FastestConsecutiveLapCalculatorTests
{

    private (DateTime finishTime, uint finishLapNumber, long totalSpacing) AddConsecutiveLaps(List<Lap> laps, DateTime start, uint startLapNumber, params long[] spacing)
    {
        var started = start;
        var lapNumber = startLapNumber;
        long totalSpacing = 0;
        DateTime? finishTime = null;
        foreach (var space in spacing)
        {
            finishTime = started.AddMilliseconds(space);
            laps.Add(new Lap(Guid.NewGuid(), started, finishTime.Value, LapStatus.Completed, space));
            started = started.AddMilliseconds(space);
            totalSpacing += space;
            lapNumber++;
        }
        if (finishTime == null)
        {
            throw new InvalidOperationException("No laps were added");
        }

        return (finishTime.Value, lapNumber, totalSpacing);
    }
    private (DateTime finishTime, uint finishLapNumber) AddInvalidLap(List<Lap> laps, DateTime start, uint startLapNumber, long spacing)
    {
        var started = start;
        var lapNumber = startLapNumber;
        var finishTime = started.AddMilliseconds(spacing);
        laps.Add(new Lap(Guid.NewGuid(), started, finishTime, LapStatus.Invalid, spacing));
        return (finishTime, lapNumber + 1);
    }
    [Fact]
    public void When_3DecrementingFastestLapsAtTheEnd_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 10000, 9700, 9500, 9000, 8500, 8000);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.NotNull(position);
        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);

    }
    [Fact]
    public void When_5DecrementingFastestLapsAtTheEnd_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 100, 200, 300, 400, 500, 50, 40, 30, 20, 10);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(5, laps);

        Assert.Equal(50 + 40 + 30 + 20 + 10, position.TotalMilliseconds);
        Assert.Equal((uint)5, position.TotalLaps);


    }
    [Fact]
    public void When_3FasterButNotConsecutive_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        (start, var lap, _) = AddConsecutiveLaps(laps, start, 0, 10000, 10000, 10000);
        (start, lap) = AddInvalidLap(laps, start, lap, 5000);
        (start, lap, _) = AddConsecutiveLaps(laps, start, lap, 5000, 5000);
        (start, lap) = AddInvalidLap(laps, start, lap, 5000);
        (start, lap, _) = AddConsecutiveLaps(laps, start, lap, 5000, 5000);


        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(10000 + 10000 + 10000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);


    }
    [Fact]
    public void When_3FastestDecrementingLapsAtTheStart_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 9000, 8500, 8000, 10000, 12000, 14000);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);


    }
    [Fact]
    public void When_3FastestDecrementingLapsInTheMiddle_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 25000, 26000, 30000, 9000, 8500, 8000, 10000, 12000, 14000);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);


    }
    [Fact]
    public void When_3FastestAllTheSameBarOneAtTheEnd_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 1000, 1000, 1000, 1000, 1000, 999);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(1000 + 1000 + 999, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);


    }


    [Fact]
    public void When_OnlyOneLap_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(laps, start, 0, 1000);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(1000, position.TotalMilliseconds);
        Assert.Equal((uint)1, position.TotalLaps);


    }
    [Fact]
    public void When_SecondOfTwoNonConsecutiveLaps_Expect_CorrectResults()
    {
        var laps = new List<Lap>();
        var start = new DateTime(1990, 1, 1);

        (var startDate, var startNumber, _) = AddConsecutiveLaps(laps, start, 0, 1000);
        (startDate, startNumber) = AddInvalidLap(laps, startDate, startNumber, 1000);
        AddConsecutiveLaps(laps, startDate, startNumber, 500);

        var calculator = new FastestConsecutiveLapCalculator();

        var position = calculator.CalculateFastestConsecutiveLaps(3, laps);

        Assert.Equal(500, position.TotalMilliseconds);
        Assert.Equal((uint)1, position.TotalLaps);


    }
}
