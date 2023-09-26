using ImmersionRC.LapRF.Protocol;

namespace ImmersionRC.LapRF.Tests;

public class StatusProtocolTests
{
    [Fact]
    public void Test1()
    {
        string base64Data = "WmEAMq4K2iECwjsjAQEkBAAAAAABAQEiBAAAYUQBAQIiBAAAYUQBAQMiBACAbUQBAQQiBABAZEQBAQUiBACAZEQBAQYiBAAAgkQBAQciBAAAZ0QBAQgiBAAAAAADAgQAWw==";
        ushort voltage = 15298;
        byte gateState = 1;
        uint statusCount = 0;
        ushort status = 4;

        float[] rssiLevel = { 900, 900, 950, 913, 914, 1040, 924, 0 };

        byte[] data = Convert.FromBase64String(base64Data);

        var packetData = data.AsSpan().Slice(7);

        var reader = new ReadOnlySpanReader<byte>(packetData);

        var protocol = new StatusProtocol();

        protocol.HandleRecordData(reader);

        Assert.Equal(voltage, protocol.CurrentStatus?.InputVoltage);
        Assert.Equal(gateState, protocol.CurrentStatus?.GateState);
        Assert.Equal(statusCount, protocol.CurrentStatus?.StatusCount);
        Assert.Equal(status, protocol.CurrentStatus?.StatusFlags);

        for (byte i = 0; i < rssiLevel.Length; i++)
        {
            var transponderRssi = protocol.GetLastReceivedSignalStrengthIndicator((byte)(i + 1));
            Assert.NotNull(transponderRssi);
            Assert.Equal(rssiLevel[i], transponderRssi.Value.Level);
        }
    }
}