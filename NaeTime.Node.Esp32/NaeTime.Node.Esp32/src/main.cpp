#include <Arduino.h>
#include "debug.h"
#include "timer/Rx5808RssiTimerNode.h"
#include <Preferences.h>
#include <vector>
#include "CommandPackets.h"
#include "comms/NetworkCommunication.h"

#define TRANSMISSION_DELAY_HZ 50
#define TRANSMISSION_ALIVE_HZ 1

#define STATUS_LED_RED D5
#define STATUS_LED_GREEN D4
#define BROADCAST_DELAY_HZ 1
#define DEVICE_ID_KEY "Device_ID"

Preferences preferences;
NetworkCommunication comms;

class LEDStatus{
  public:
    LEDStatus(int pin,int onValue,int offValue) {
      Pin = pin;
      OnValue = onValue;
      OffValue = offValue;
    }
    int Pin;
    int OnValue;
    int OffValue;
    long LastChange = 0;
    bool On = false;
    bool Enabled = false;
};
LEDStatus *onboard = new LEDStatus(LED_BUILTIN,HIGH,LOW);
LEDStatus *onboardRed = new LEDStatus(LED_RED,LOW,HIGH);
LEDStatus *onboardGreen = new LEDStatus(LED_GREEN,LOW,HIGH);
LEDStatus *onboardBlue = new LEDStatus(LED_BLUE,LOW,HIGH);
LEDStatus *statusRed = new LEDStatus(STATUS_LED_RED,HIGH,LOW);
LEDStatus *statusGreen = new LEDStatus(STATUS_LED_GREEN,HIGH,LOW);

const long transmissionPeriod = 1000000 / TRANSMISSION_DELAY_HZ;
const long alivePeriod = 1000 / TRANSMISSION_ALIVE_HZ;
const long broadcastPeriod = 1000 / BROADCAST_DELAY_HZ;

long lastAliveTick = 0;
long lastBroadcast = 0;
bool wifiConnectedLastTick = false;


int nodeCount = 4;
Rx5808RssiTimerNode nodes[] = {Rx5808RssiTimerNode(A0, D3, D2, D12), Rx5808RssiTimerNode(A1, D3, D2, D11), Rx5808RssiTimerNode(A3, D3, D2, D10), Rx5808RssiTimerNode(A4, D3, D2, D9)};

void FlashLED(LEDStatus* status, long currentMillis,int delay){
  if(!status->Enabled){
    if(status->On)
    {
      digitalWrite(status->Pin, status->OffValue);
      status->On = false;
    }
  }
  else if(delay == 0){
    if(status->On)
    {
      digitalWrite(status->Pin, status->OnValue);
      status->On = true;
    }
  }
  else 
  {
    
    if((currentMillis - status->LastChange) > delay)
      {
          if(status->On){
            digitalWrite(status->Pin, status->OffValue);
            status->LastChange = currentMillis;
            status->On = false;
          }
          else{
            digitalWrite(status->Pin, status->OnValue);
            status->LastChange = currentMillis;
            status->On = true;
          }
      }
  }
}
void FlashWIFILED(long currentMillis,LEDStatus* status){
  int delay = 0;
  if(!comms.IsConnectedToNetwork()){
    status->Enabled = true;
    delay = 1000;
  }
  else{
    status->Enabled = false;
  }
  FlashLED(status,currentMillis,delay);
}
void FlashConnectionLED(long currentMillis,LEDStatus* status){
  int delay = 1000;
  if(comms.IsConnectedToNetwork()){
    status->Enabled = true;
    if(comms.IsConnectedToClient()){
      delay = 0;
    }
  }
  else{
    status->Enabled = false;
  }
  
  FlashLED(status,currentMillis,delay);
}
void FlashLEDs(){
  long current = millis();

  FlashWIFILED(current, onboardRed);
  FlashWIFILED(current, statusRed);

  FlashConnectionLED(current, onboardGreen);
  FlashConnectionLED(current, statusGreen);
}
void initRx58080Modules(){
  for(int i =0; i< nodeCount; i++){
    nodes[i].Init();
  }
}
void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(LED_BLUE,OUTPUT);
  pinMode(LED_GREEN,OUTPUT);
  pinMode(LED_RED,OUTPUT);
  pinMode(STATUS_LED_GREEN, OUTPUT);
  pinMode(STATUS_LED_RED, OUTPUT);

  digitalWrite(LED_BUILTIN, HIGH);

  preferences.begin("naetime-node");
  comms.Init(preferences.getString(DEVICE_ID_KEY).c_str(),20);

  Serial.begin(9600);
  

  debugprintln("Init nodes");
  initRx58080Modules();

  debugprintln("Setup complete");
}

void transmitNodeRssi(WiFiClient client,int nodeIndex,long tick, int rssi){
  NodeRssi packet;
  packet.NodeIndex = nodeIndex;
  packet.Tick = tick;
  packet.Rssi = rssi;

  PacketHeader header;
  header.CommandType = RSSI_TICK_COMMAND;
  header.DataLength = sizeof(NodeRssi);

  char* headerData = (char*)&header;
  char* packetData = (char*)&packet;

  client.write(headerData, sizeof(PacketHeader));
  client.write(packetData, sizeof(NodeRssi));
}
void transmitNodeEnteredCrossing(WiFiClient client,int nodeIndex,long tick, int rssi){
  NodeEnteredCrossing packet;
  packet.NodeIndex = nodeIndex;
  packet.Tick = tick;
  packet.CrossingCount = 0;

  PacketHeader header;
  header.CommandType = CROSSING_ENTERED_COMMAND;
  header.DataLength = sizeof(NodeEnteredCrossing);

  char* headerData = (char*)&header;
  char* packetData = (char*)&packet;

  client.write(headerData, sizeof(PacketHeader));
  client.write(packetData, sizeof(NodeEnteredCrossing));
}
void transmitNodeLeftCrossing(WiFiClient client,int nodeIndex,long tick, int rssi){
  NodeLeftCrossing packet;
  packet.NodeIndex = nodeIndex;
  packet.Tick = tick;
  packet.CrossingCount = 0;

  PacketHeader header;
  header.CommandType = CROSSING_LEFT_COMMAND;
  header.DataLength = sizeof(NodeLeftCrossing);

  char* headerData = (char*)&header;
  char* packetData = (char*)&packet;

  client.write(headerData, sizeof(PacketHeader));
  client.write(packetData, sizeof(NodeLeftCrossing));
}
void transmitAlive(WiFiClient client,long current){
  Alive packet;
  packet.Tick = current;

  PacketHeader header;
  header.CommandType = ALIVE_COMMAND;
  header.DataLength = sizeof(Alive);
  header.Data = (char*)&packet;

  unsigned int dataSize = sizeof(Alive) + sizeof(PacketHeader);
  uint8_t* data = (uint8_t*)&header;
  data = (uint8_t*)&header;

  client.write(data, sizeof(Alive) + sizeof(PacketHeader));
}

void DoClientConnectedLoop(){
  long current = millis();
  if(current - lastAliveTick > broadcastPeriod){
    comms.TransmitAlive(current);
    lastAliveTick = current;
  }

  for(int i =0; i< nodeCount; i++){
    nodes[i].TickRssi();

    RssiNodeState currentState = nodes[i].GetCurrentState();

    long timeSinceLastTransmission = currentState.LastTick - currentState.LastTransmittedTick;

    if(timeSinceLastTransmission > transmissionPeriod && currentState.CurrentState != BELOW_TRANSMISSION_THRESHOLD){
        comms.TransmitNodeRssi(i,currentState.LastTick,currentState.LastFilteredRssi);
        nodes[i].MarkTickAsTrasmitted();
    }

    if(currentState.CurrentState == ENTERED_CROSSING){
      comms.TransmitNodeEnteredCrossing(i,currentState.LastTick,currentState.LastFilteredRssi);
    }
    else if(currentState.CurrentState == LEFT_CROSSING){
      comms.TransmitNodeLeftCrossing(i,currentState.LastTick,currentState.LastFilteredRssi);
    }
  }
}
void BroadcastClientIpAndConfig(long currentMillis){

}
void DoNoClientLoop(){
  long current = millis();

  if(current - lastBroadcast > broadcastPeriod){
    comms.BroadcastDeviceInfo(current);
    lastBroadcast = current;
  }
}
void DoWifiConnectedLoop(){
    if(comms.IsConnectedToClient()){
      DoClientConnectedLoop();
    }
    else{
      DoNoClientLoop();
    }
}

void loop(){
  comms.Tick();

  FlashLEDs();
  if(comms.IsConnectedToNetwork()){
    DoWifiConnectedLoop();
  }
  else{
    wifiConnectedLastTick = false;
  }
}

