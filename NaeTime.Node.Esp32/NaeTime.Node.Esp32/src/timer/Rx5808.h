#define SynthesizerA 0x00
#define SynthesizerB 0x01
#define SynthesizerC 0x02
#define SynthesizerD 0x03
#define VCO_Switch_Cap_Control 0x04
#define DFC_Control 0x05
#define SixM_Audio_Demodulator_Control 0x06
#define SixM_Five_Audio_Demodulator_Control 0x07
#define Receiver_Control_One 0x08
#define Receiver_Control_Two 0x09
#define Power_Down_Control 0x0A
#define State 0x0F

class Rx5808 {
    int _rssiPin;
    int _clockPin;
    int _dataPin;
    int _selectPin;

    bool _isDataInWrite;

    public: 
        Rx5808(int rssiPin, int clockPin, int dataPin, int selectPin);
        void Init();
        void SetFrequency(int frequency);
        bool ConfirmFrequency(int frequency);
        int GetActualStoredFrequency();
        bool Tune(int frequency);
        int GetRssi();

    private:
        void WriteBit(bool value);
        bool ReadBit();
        void PulseClockPin();
        void SetSelect(bool isHigh);
        void SetupDataPinForRead();
        void SetupDataPinForWrite();
        void SendRegisterAddress(int registerAddress);
        int ReadFromRegister(int registerAddress);
        void WriteToRegister(int registerAddress, int value);
        int CalculateFrequencyRegisterValue(int frequencyInMhz);
        int CalculateRegisterValueFrequency(int registerValue);
        bool DoFrequenciesMatch(int frequencyOne, int frequencyTwo);
};