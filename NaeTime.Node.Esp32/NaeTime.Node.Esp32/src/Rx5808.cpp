#include "Rx5808.h"
#include <Arduino.h>

#define COMMUNICATION_DELAY_MICROSECONDS 10

Rx5808::Rx5808(int rssiPin, int clockPin, int dataPin, int selectPin){
    _rssiPin = rssiPin;
    _clockPin = clockPin;
    _dataPin = dataPin;
    _selectPin = selectPin;   
}
void Rx5808::Init(){
    pinMode(_rssiPin, INPUT);
    pinMode(_clockPin, OUTPUT);
    pinMode(_dataPin, OUTPUT);
    _isDataInWrite = true;
    pinMode(_selectPin, OUTPUT);
}
void Rx5808::SetFrequency(int frequencyInMhz){
    int registerValue = CalculateFrequencyRegisterValue(frequencyInMhz);

    WriteToRegister(SynthesizerB, registerValue);
}
bool Rx5808::ConfirmFrequency(int frequency){
    int registerValue = ReadFromRegister(SynthesizerB);

    return registerValue == CalculateFrequencyRegisterValue(frequency);
}
int Rx5808::GetActualStoredFrequency(){
    int registerValue = ReadFromRegister(SynthesizerB);

    return CalculateRegisterValueFrequency(registerValue);
}
int Rx5808::GetRssi(){
    return analogRead(_rssiPin);
}

void Rx5808::PulseClockPin(){

    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    digitalWrite(_clockPin, HIGH);
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    digitalWrite(_clockPin,LOW);
}
void Rx5808::SetSelect(bool isHigh){
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    if(isHigh){
        digitalWrite(_selectPin, HIGH);
    }else{
        digitalWrite(_selectPin, LOW);
    }
}
void Rx5808::WriteBit(bool value){
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    if(value){
        digitalWrite(_dataPin, HIGH);
    }else{
        digitalWrite(_dataPin,LOW);
    }
    PulseClockPin();
}
bool Rx5808::ReadBit(){
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    int pinValue = digitalRead(_dataPin);
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    PulseClockPin();
    return pinValue == HIGH;
}
void Rx5808::SetupDataPinForRead(){
    if(!_isDataInWrite){
        return;
    }

    _isDataInWrite = false;
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    pinMode(_dataPin, INPUT_PULLUP);
}
void Rx5808::SetupDataPinForWrite(){
    if(_isDataInWrite){
        return;
    }

    _isDataInWrite =true;
    delayMicroseconds(COMMUNICATION_DELAY_MICROSECONDS);
    pinMode(_dataPin, OUTPUT);
}
void Rx5808::SendRegisterAddress(int registerAddress){
    for (byte i = 0; i < 4; i++)
    {
        bool readBit = (((byte)registerAddress >> i) & 0x01) == 0x01;
        WriteBit(readBit);
    }
}
int Rx5808::ReadFromRegister(int registerAddress){
    SetSelect(true); //Raise the select before a transaction to ensure that the trailing edge proceeds the transaction
    
    SetupDataPinForWrite();
    SetSelect(false);

    //Write register address
    SendRegisterAddress(registerAddress);

    //We are reading from register
    WriteBit(false);

    SetupDataPinForRead();

    int result = 0;
    for (byte location = 0; location < 20; location++)
    {
        bool readValue = ReadBit();
        if (readValue)
        {
            result |= 1 << location;
        }
    }
    SetSelect(true);
    return result;
}
void Rx5808::WriteToRegister(int registerAddress, int value){
    SetSelect(true); //Raise the select before a transaction to ensure that the trailing edge proceeds the transaction
    SetupDataPinForWrite();
    SetSelect(false);

    //Write register address
    SendRegisterAddress(registerAddress);

    WriteBit(true);

    for (byte location = 0; location < 20; location++)
    {
        if ((value & (1 << location)) == (1 << location))
        {
            WriteBit(true);
        }
        else
        {
            WriteBit(false);
        }
    }
    SetSelect(true);
}
int Rx5808::CalculateFrequencyRegisterValue(int frequencyInMhz){
    int tf = (frequencyInMhz - 479) / 2;
    int N = tf / 32;
    int A = tf % 32;
    return (N << 7) + A;
}
int Rx5808::CalculateRegisterValueFrequency(int registerValue){
    int N = registerValue >> 7;
    int A = registerValue & 0x7F;

    int tf = (N * 32) + A;

    return (tf * 2) + 479;
}
bool Rx5808::DoFrequenciesMatch(int frequencyOne,int frequencyTwo){
    return CalculateRegisterValueFrequency(frequencyOne) 
        == CalculateFrequencyRegisterValue(frequencyTwo);
}