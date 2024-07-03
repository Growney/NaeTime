#include <Arduino.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <CircularBuffer.hpp>
#include "debug.h"
#include "timer/Rx5808RssiTimerNode.h"

#define TRANSMISSION_DELAY_HZ 50
#define TRANSMISSION_ALIVE_HZ 1
#define ALIVE_COMMAND 0
#define RSSI_TICK_COMMAND 1
#define CROSSING_ENTERED_COMMAND 2
#define CROSSING_LEFT_COMMAND 3

const char * networkSid = "Beagle WIFI";
const char * networkPassword = "whatcanthebeaglesmellunderthetree";

const char * udpAddress = "192.168.0.124";
const int udpPort = 11000;

long lastLedChange = 0;
bool ledOn = false;
//Are we currently connected?
boolean connected = false;

long transmissionPeriod = 1000000 / TRANSMISSION_DELAY_HZ;
long alivePeriod = 1000 / TRANSMISSION_ALIVE_HZ;

long lastAliveTick = 0;

WiFiUDP udp;

int nodeCount = 4;
Rx5808RssiTimerNode nodes[] = {Rx5808RssiTimerNode(A0, D3, D2, D12), Rx5808RssiTimerNode(A1, D3, D2, D11), Rx5808RssiTimerNode(A3, D3, D2, D10), Rx5808RssiTimerNode(A4, D3, D2, D9)};

void WiFiEvent(WiFiEvent_t event){
    switch(event) {
      case ARDUINO_EVENT_WIFI_STA_GOT_IP:
          //When connected set 
          debugprint("WiFi connected! IP address: ");
          debugprintln(WiFi.localIP());  
          //initializes the UDP state
          //This initializes the transfer buffer
          udp.begin(WiFi.localIP(),udpPort);
          connected = true;
          break;
      case ARDUINO_EVENT_WIFI_STA_DISCONNECTED:
          debugprintln("WiFi lost connection");
          connected = false;
          break;
      default: break;
    }
}
void connectToWiFi(const char * ssid, const char * pwd){
  debugprintln("Connecting to WiFi network: " + String(ssid));

  // delete old config
  WiFi.disconnect(true);
  //register event handler
  WiFi.onEvent(WiFiEvent);
  
  //Initiate connection
  WiFi.begin(ssid, pwd);

  debugprintln("Waiting for WIFI connection...");
}
void FlashLED(){
  long current = millis();
  int delay = 1000;
  if(!connected){
    delay = 100;
  }

  if((current - lastLedChange) > delay)
  {
      if(ledOn){
        digitalWrite(LED_BUILTIN, LOW);
        lastLedChange = current;
        ledOn = false;
      }
      else{
        digitalWrite(LED_BUILTIN, HIGH);
        lastLedChange = current;
        ledOn = true;
      }
  }
}
void initRx58080Modules(){
  for(int i =0; i< nodeCount; i++){
    nodes[i].Init();
  }
}
void setup() {
  Serial.begin(9600);
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(LED_BLUE,OUTPUT);
  pinMode(LED_GREEN,OUTPUT);
  pinMode(LED_RED,OUTPUT);

  digitalWrite(LED_BLUE, HIGH);
  digitalWrite(LED_GREEN, HIGH);
  digitalWrite(LED_RED, LOW);

  digitalWrite(LED_BUILTIN, HIGH);

  debugprintln("Init nodes");
  initRx58080Modules();
  
  debugprintln("Connect to wifi");
  connectToWiFi(networkSid, networkPassword);
  
  digitalWrite(LED_BUILTIN, LOW);
  digitalWrite(LED_GREEN, LOW);
  digitalWrite(LED_RED, HIGH);

  debugprintln("Setup complete");
}


void writeUdp(WiFiUDP* udp,int value){
  int segments = sizeof(int) / sizeof(uint8_t);
  for(int i = 0; i < segments; i ++){
    int shiftBy =(i * sizeof(uint8_t) * 8);
    int shifted = value >> shiftBy;
    udp->write(shifted);
  }
}
void writeUdp(WiFiUDP* udp,long value){
  int segments = sizeof(long) / sizeof(uint8_t);
  for(int i = 0; i < segments; i ++){
    int shiftBy =(i * sizeof(uint8_t) * 8);
    int shifted = value >> shiftBy;
    udp->write(shifted);
  }
}

void transmitNodeRssi(WiFiUDP* udp,int nodeIndex,long tick, int rssi){
  udp->beginPacket(udpAddress,udpPort);
  udp->write(RSSI_TICK_COMMAND);
  writeUdp(udp,nodeIndex);
  writeUdp(udp,tick);
  writeUdp(udp,rssi);
  udp->endPacket();
}
void transmitNodeEnteredCrossing(WiFiUDP* udp,int nodeIndex,long tick, int rssi){
  udp->beginPacket(udpAddress,udpPort);
  udp->write(CROSSING_ENTERED_COMMAND);
  writeUdp(udp,nodeIndex);
  writeUdp(udp,tick);
  writeUdp(udp,rssi);
  udp->endPacket();
}
void transmitNodeLeftCrossing(WiFiUDP* udp,int nodeIndex,long tick, int rssi){
  udp->beginPacket(udpAddress,udpPort);
  udp->write(CROSSING_LEFT_COMMAND);
  writeUdp(udp,nodeIndex);
  writeUdp(udp,tick);
  writeUdp(udp,rssi);
  udp->endPacket();
}
void transmitAlive(WiFiUDP* udp){
  udp->beginPacket(udpAddress,udpPort);
  udp->write(ALIVE_COMMAND);
  udp->endPacket();
}
void loop(){
  FlashLED();

  if(connected){
      for(int i =0; i< nodeCount; i++){
        nodes[i].TickRssi();

        RssiNodeState currentState = nodes[i].GetCurrentState();

        long timeSinceLastTransmission = currentState.LastTick - currentState.LastTransmittedTick;

        if(timeSinceLastTransmission > transmissionPeriod && currentState.CurrentState != BELOW_TRANSMISSION_THRESHOLD){
           transmitNodeRssi(&udp,i,currentState.LastTick,currentState.LastFilteredRssi);
           nodes[i].MarkTickAsTrasmitted();
        }

        if(currentState.CurrentState == ENTERED_CROSSING){
          transmitNodeEnteredCrossing(&udp,i,currentState.LastTick,currentState.LastFilteredRssi);
        }
        else if(currentState.CurrentState == LEFT_CROSSING){
          transmitNodeLeftCrossing(&udp,i,currentState.LastTick,currentState.LastFilteredRssi);
        }
      }
      long currentTick = millis();
      long timeSinceLastAlive = currentTick - lastAliveTick;

      if(timeSinceLastAlive > alivePeriod){
        transmitAlive(&udp);
        lastAliveTick =currentTick;
      }
    }
}

