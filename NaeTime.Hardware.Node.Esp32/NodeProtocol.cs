﻿using NaeTime.Bytes;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeProtocol : INodeProtocol
{
    public const byte START_OF_RECORD = 0x5a;
    public const byte END_OF_RECORD = 0x5b;
    public const byte ESCAPE = 0x5c;
    public const byte ESCAPED_ADDER = 0x40;
    public static readonly DataEscaper DataEscaper = new(ESCAPE, ESCAPED_ADDER, 1, 1, START_OF_RECORD, END_OF_RECORD, ESCAPE);

    private readonly INodeCommunication _communication;
    public INodeTimingProtocol TimingProtocol { get; }
    private readonly Crc16 _crc16 = new();

    private readonly Stack<byte> _receivedStack = new();

    private readonly byte[] _packet = new byte[2096];

    public NodeProtocol(INodeCommunication communication, INodeTimingProtocol timingProtocol)
    {
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        TimingProtocol = timingProtocol;
    }

    public async Task RunAsync(CancellationToken token)
    {
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(10));

        while (!token.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(token).ConfigureAwait(false);

            ReadOnlyMemory<byte> data = await _communication.ReceiveAsync(token).ConfigureAwait(false);

            if (!token.IsCancellationRequested)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    ProcessByte(data.Span[i]);
                }
            }
        }
    }

    private void ProcessByte(byte dataByte)
    {
        if (dataByte == START_OF_RECORD)
        {
            HandleStartOfRecord();
        }
        else if (dataByte == END_OF_RECORD)
        {
            HandleEndOfRecord();
        }
        else
        {
            _receivedStack.Push(dataByte);
        }
    }

    private void HandleStartOfRecord()
    {
        if (_receivedStack.TryPeek(out byte previousByte))
        {
            if (previousByte != ESCAPE)
            {
                _receivedStack.Clear();
            }
        }

        _receivedStack.Push(START_OF_RECORD);
    }
    private void HandleEndOfRecord()
    {
        if (_receivedStack.TryPeek(out byte previousByte))
        {
            if (previousByte != ESCAPE)
            {
                _receivedStack.Push(END_OF_RECORD);
                ReadOnlySpan<byte> packetData = BuildRecord();
                HandleRecord(packetData);
            }
        }
    }

    private ReadOnlySpan<byte> BuildRecord()
    {
        int index = _receivedStack.Count - 1;
        int recordLength = 0;
        while (_receivedStack.Count > 0)
        {
            _packet[index] = _receivedStack.Pop();
            index--;
            recordLength++;
        }

        return _packet.AsSpan()[..recordLength];
    }

    private void HandleRecord(ReadOnlySpan<byte> packetSpan)
    {
        byte[] packetData = DataEscaper.UnEscape(packetSpan);

        if (packetData[0] != START_OF_RECORD)
        {
            //something has gone wrong the record should always start with a start of record
            return;
        }

        if (packetData[^1] != END_OF_RECORD)
        {
            //something has gone wrong the record should always end with an end of record
            return;
        }

        RecordHeader recordHeader = RecordHeader.Read(packetData);
        RecordHeader.SetRecordCRC(packetData, 0);

        ushort recordCRC = _crc16.Compute(packetData, packetData.Length);

        //if (recordCRC != recordHeader.RecordCRC)
        //{
        //    //invalid crc
        //    return;
        //}

        Span<byte> recordData = packetData.AsSpan()[RecordHeader.GetDataStartLocation()..];

        ReadOnlySpanReader<byte> recordReader = new(recordData);

        switch (recordHeader.RecordType)
        {
            case RecordType.LANE_TIMINGS:
                TimingProtocol.HandleRecordData(recordReader);
                break;
            default:
                break;
        }
    }
}
