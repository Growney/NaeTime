using NaeTime.Server.Domain;

namespace NaeTime.Server.Tests;

public class OrderedStreamTests
{
    [Fact]
    public void ProcessNext_ReturnsItemsInOrder()
    {
        // Arrange
        var stream = new OrderedStream<string>();

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(1, "Apple"));
        results.AddRange(stream.ProcessNext(2, "Banana"));
        results.AddRange(stream.ProcessNext(3, "Carrot"));
        results.AddRange(stream.ProcessNext(4, "Doughnut"));

        // Assert
        var expected = new List<string> { "Apple", "Banana", "Carrot", "Doughnut" };
        Assert.Equal(expected, results);
    }

    [Fact]
    public void ProcessNext_ReturnsItemsInOrder_WithThreshold()
    {
        // Arrange
        var stream = new OrderedStream<string>(2);

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(1, "Apple"));
        results.AddRange(stream.ProcessNext(5, "Banana"));
        results.AddRange(stream.ProcessNext(2, "Carrot"));
        results.AddRange(stream.ProcessNext(3, "Doughnut"));
        results.AddRange(stream.ProcessNext(4, "Eggplant"));

        // Assert
        var expected = new List<string> { "Apple", "Doughnut", "Eggplant", "Banana" };
        Assert.Equal(expected, results);
    }

    [Fact]
    public void ProcessNext_ReturnsItemsInOrder_WithOutOfOrderValues()
    {
        // Arrange
        var stream = new OrderedStream<string>();

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(2, "Apple"));
        results.AddRange(stream.ProcessNext(5, "Banana"));
        results.AddRange(stream.ProcessNext(3, "Carrot"));
        results.AddRange(stream.ProcessNext(4, "Doughnut"));
        results.AddRange(stream.ProcessNext(6, "Eggplant"));

        // Assert
        var expected = new List<string> { "Apple", "Carrot", "Doughnut", "Banana", "Eggplant" };
        Assert.Equal(expected, results);
    }

    [Fact]
    public void ProcessNext_ReturnsItemsInOrder_WithLargeGap()
    {
        // Arrange
        var stream = new OrderedStream<string>();

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(10, "10"));
        results.AddRange(stream.ProcessNext(20, "20"));
        results.AddRange(stream.ProcessNext(15, "15"));
        results.AddRange(stream.ProcessNext(17, "17"));
        results.AddRange(stream.ProcessNext(12, "12"));
        results.AddRange(stream.ProcessNext(13, "13"));

        // Assert
        var expected = new List<string> { };
        Assert.Equal(expected, results);


        results.AddRange(stream.ProcessNext(16, "16"));
        results.AddRange(stream.ProcessNext(18, "18"));
        results.AddRange(stream.ProcessNext(11, "11"));
        results.AddRange(stream.ProcessNext(19, "19"));
        results.AddRange(stream.ProcessNext(14, "14"));

        expected = new List<string> { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };
        Assert.Equal(expected, results);

    }

    [Fact]
    public void ProcessNext_StartValueFromFirstValue()
    {
        // Arrange
        var stream = new OrderedStream<string>();

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(5, "Banana"));
        var expected = new List<string> { };
        Assert.Equal(expected, results);

        results.AddRange(stream.ProcessNext(1, "Apple"));
        expected = new List<string> { };
        Assert.Equal(expected, results);

        results.AddRange(stream.ProcessNext(2, "Carrot"));
        expected = new List<string> { };
        Assert.Equal(expected, results);

        results.AddRange(stream.ProcessNext(4, "Spaceman"));
        expected = new List<string> { };
        Assert.Equal(expected, results);
        results.AddRange(stream.ProcessNext(3, "Doughnut"));
        // Assert
        expected = new List<string> { "Apple", "Carrot", "Doughnut", "Spaceman", "Banana", };
        Assert.Equal(expected, results);


        results.AddRange(stream.ProcessNext(6, "Spagbol"));
        // Assert
        expected = new List<string> { "Apple", "Carrot", "Doughnut", "Spaceman", "Banana", "Spagbol" };
        Assert.Equal(expected, results);
    }

    [Fact]
    public void ProcessNext_ClearsBuffer_WhenThresholdExceeded()
    {
        // Arrange
        var stream = new OrderedStream<string>(2);

        // Act
        var results = new List<string>();
        results.AddRange(stream.ProcessNext(1, "Apple"));
        results.AddRange(stream.ProcessNext(5, "Banana"));
        results.AddRange(stream.ProcessNext(2, "Carrot"));
        results.AddRange(stream.ProcessNext(3, "Doughnut"));
        results.AddRange(stream.ProcessNext(4, "Eggplant"));
        results.AddRange(stream.ProcessNext(8, "Fig")); // Threshold exceeded, should clear buffer
        results.AddRange(stream.ProcessNext(7, "Grape")); // Buffer cleared, should be processed immediately
        results.AddRange(stream.ProcessNext(6, "Pineapple"));

        // Assert
        var expected = new List<string> { "Apple", "Doughnut", "Eggplant", "Banana", "Grape", "Fig" };
        Assert.Equal(expected, results);
    }
}