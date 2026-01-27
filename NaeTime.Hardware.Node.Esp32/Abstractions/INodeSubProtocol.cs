using NaeTime.Bytes;

namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeSubProtocol
{
    internal void HandleRecordData(RecordType commandId, ReadOnlySpanReader<byte> recordReader);
    internal void HandleResponseData(RecordType responseCode,RecordType commandId, ReadOnlySpanReader<byte> responseReader);
}
