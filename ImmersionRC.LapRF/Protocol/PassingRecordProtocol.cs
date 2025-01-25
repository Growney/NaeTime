using ImmersionRC.LapRF.Abstractions;
using NaeTime.Bytes;
using NaeTime.Collections;

namespace ImmersionRC.LapRF.Protocol;
internal class PassingRecordProtocol : IPassingRecordProtocol
{
    private readonly AwaitableQueue<Pass?> _passes = new(50);

    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte? pilotId = null;
        uint? passingNumber = null;
        ulong? realTimeClockTime = null;

        while (recordReader.HasData())
        {
            byte fieldSignature = recordReader.ReadByte();

            if (fieldSignature == LapRFProtocol.END_OF_RECORD)
            {
                return;
            }
            ///read this to clear off the buffer but i am not going to check it because i don't think it makes a difference if it does then i will have to change this
            _ = recordReader.ReadByte();

            switch ((ProtocolFields)fieldSignature)
            {
                case ProtocolFields.TransponderId:
                    pilotId = recordReader.ReadByte();
                    break;
                case ProtocolFields.PassNumber:
                    passingNumber = recordReader.ReadUInt32();
                    break;
                case ProtocolFields.Timestamp:
                    realTimeClockTime = recordReader.ReadUInt64();
                    break;
                default:
                    break;
            }

            if (pilotId != null && passingNumber != null && realTimeClockTime != null)
            {
                _passes.Enqueue(new Pass(passingNumber.Value, pilotId.Value, 0, 0, realTimeClockTime.Value));
                pilotId = null;
                passingNumber = null;
                realTimeClockTime = null;
            }
        }
    }

    public Task<Pass?> WaitForNextPassAsync(CancellationToken cancellationToken)
    {
        return _passes.WaitForDequeueAsync(cancellationToken);
    }
}
