using NaeTime.Bytes;

namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeSubProtocol
{
    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
    internal void HandleResponseData(byte responseCode, ReadOnlySpanReader<byte> responseReader);
}
