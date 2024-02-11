using NaeTime.Timing.Practice;

namespace NaeTime.Timing.Tests;
public class OpenPracticeConsecutiveTests
{

    private (DateTime finishTime, uint finishLapNumber, long totalSpacing) AddConsecutiveLaps(OpenPracticeSession session, Guid pilotId, DateTime start, uint startLapNumber, params long[] spacing)
    {
        var started = start;
        var lapNumber = startLapNumber;
        long totalSpacing = 0;
        DateTime? finishTime = null;
        foreach (var space in spacing)
        {
            finishTime = started.AddMilliseconds(space);
            session.AddCompletedLap(pilotId, lapNumber, started, finishTime.Value, space);
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
    private (DateTime finishTime, uint finishLapNumber) AddInvalidLap(OpenPracticeSession session, Guid pilotId, DateTime start, uint startLapNumber, long spacing)
    {
        var started = start;
        var lapNumber = startLapNumber;
        var finishTime = started.AddMilliseconds(spacing);
        session.AddTooLongLap(pilotId, lapNumber, started, finishTime, spacing);
        return (finishTime, lapNumber + 1);
    }
    [Fact]
    public void When_3DecrementingFastestLapsAtTheEnd_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 10000, 9700, 9500, 9000, 8500, 8000);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)3, position.StartLapNumber);
        Assert.Equal((uint)5, position.EndLapNumber);

    }
    [Fact]
    public void When_5DecrementingFastestLapsAtTheEnd_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 100, 200, 300, 400, 500, 50, 40, 30, 20, 10);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(5);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(50 + 40 + 30 + 20 + 10, position.TotalMilliseconds);
        Assert.Equal((uint)5, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)5, position.StartLapNumber);
        Assert.Equal((uint)9, position.EndLapNumber);

    }
    [Fact]
    public void When_3FasterButNotConsecutive_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        (start, var lap, _) = AddConsecutiveLaps(session, pilotId, start, 0, 10000, 10000, 10000);
        (start, lap) = AddInvalidLap(session, pilotId, start, lap, 5000);
        (start, lap, _) = AddConsecutiveLaps(session, pilotId, start, lap, 5000, 5000);
        (start, lap) = AddInvalidLap(session, pilotId, start, lap, 5000);
        (start, lap, _) = AddConsecutiveLaps(session, pilotId, start, lap, 5000, 5000);


        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(10000 + 10000 + 10000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)0, position.StartLapNumber);
        Assert.Equal((uint)2, position.EndLapNumber);

    }
    [Fact]
    public void When_3FastestDecrementingLapsAtTheStart_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 9000, 8500, 8000, 10000, 12000, 14000);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)0, position.StartLapNumber);
        Assert.Equal((uint)2, position.EndLapNumber);

    }
    [Fact]
    public void When_3FastestDecrementingLapsInTheMiddle_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 25000, 26000, 30000, 9000, 8500, 8000, 10000, 12000, 14000);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(9000 + 8500 + 8000, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)3, position.StartLapNumber);
        Assert.Equal((uint)5, position.EndLapNumber);

    }
    [Fact]
    public void When_3FastestAllTheSameBarOneAtTheEnd_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 1000, 1000, 1000, 1000, 1000, 999);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(1000 + 1000 + 999, position.TotalMilliseconds);
        Assert.Equal((uint)3, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)3, position.StartLapNumber);
        Assert.Equal((uint)5, position.EndLapNumber);

    }
    [Fact]
    public void When_MultiPilotsHaveTheSameFastest_Expect_ThePilotWhoDidItFirstToBeFirst()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotSecondPlace = Guid.NewGuid();
        var pilotSecondPlaceStart = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotSecondPlace, pilotSecondPlaceStart, 0, 1000, 1000, 1000, 1000, 1000, 999);

        var pilotFirstPlace = Guid.NewGuid();
        var pilotFirstPlaceStart = new DateTime(1989, 1, 1);

        AddConsecutiveLaps(session, pilotFirstPlace, pilotFirstPlaceStart, 0, 1000, 1000, 1000, 1000, 1000, 999);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        var firstPlace = leaderboard.First(x => x.PilotId == pilotSecondPlace);

        Assert.Equal(1000 + 1000 + 999, firstPlace.TotalMilliseconds);
        Assert.Equal((uint)3, firstPlace.TotalLaps);
        Assert.Equal((uint)1, firstPlace.Position);

        Assert.Equal((uint)3, firstPlace.StartLapNumber);
        Assert.Equal((uint)5, firstPlace.EndLapNumber);

        var secondPlace = leaderboard.First(x => x.PilotId == pilotFirstPlace);

        Assert.Equal(1000 + 1000 + 999, secondPlace.TotalMilliseconds);
        Assert.Equal((uint)3, secondPlace.TotalLaps);
        Assert.Equal((uint)0, secondPlace.Position);

        Assert.Equal((uint)3, secondPlace.StartLapNumber);
        Assert.Equal((uint)5, secondPlace.EndLapNumber);

    }

    [Fact]
    public void When_OnlyOneLap_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        AddConsecutiveLaps(session, pilotId, start, 0, 1000);

        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(1000, position.TotalMilliseconds);
        Assert.Equal((uint)1, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)0, position.StartLapNumber);
        Assert.Equal((uint)0, position.EndLapNumber);

    }
    [Fact]
    public void When_SecondOfTwoNonConsecutiveLaps_Expect_CorrectResults()
    {
        var sessionId = Guid.NewGuid();
        var session = new OpenPracticeSession(sessionId);

        var pilotId = Guid.NewGuid();
        var start = new DateTime(1990, 1, 1);

        (var startDate, var startNumber, _) = AddConsecutiveLaps(session, pilotId, start, 0, 1000);
        (startDate, startNumber) = AddInvalidLap(session, pilotId, startDate, startNumber, 1000);
        AddConsecutiveLaps(session, pilotId, startDate, startNumber, 500);
        var leaderboard = session.GetConsecutiveLapLeaderboardPositions(3);

        Assert.Single(leaderboard);

        var position = leaderboard.First();

        Assert.Equal(500, position.TotalMilliseconds);
        Assert.Equal((uint)1, position.TotalLaps);
        Assert.Equal((uint)0, position.Position);

        Assert.Equal((uint)2, position.StartLapNumber);
        Assert.Equal((uint)2, position.EndLapNumber);

    }
}
