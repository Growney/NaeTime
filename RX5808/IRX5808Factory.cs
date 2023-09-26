namespace RX5808;

public interface IRX5808Factory
{
    MultiplexedRX5808RegisterCommunication GetMultiplexedModule(byte aAddressPin, byte bAddressPin, byte cAddressPin, byte dataPin, byte selectPin, byte clockPin);
    RX5808RegisterCommunication GetSingleModuleCommunication(byte dataPin, byte selectPin, byte clockPin);
}