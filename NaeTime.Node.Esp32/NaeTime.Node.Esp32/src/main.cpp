#include <Arduino.h>
#include "Rx5808.h"
#include "NodeReading.h"
#include "filters/RssiFilter.h"
#include "filters/MeanFilter.h"
#include <WiFi.h>
#include <WiFiUdp.h>

#define DEBUG 0

#if DEBUG == 1
  #define debugprintln(x) Serial.println(x) 
  #define debugprint(x) Serial.print(x) 
#else
  #define debugprintln(x) 
  #define debugprint(x)
#endif

#define FirstMeanFilterSpan 100

const char * networkSid = "Beagle WIFI";
const char * networkPassword = "whatcanthebeaglesmellunderthetree";

const char * udpAddress = "192.168.0.124";
const int udpPort = 11000;

long lastLedChange = 0;
bool ledOn = false;
const int nodeCount = 4;
const int readingBuffer = 10;
const int filterCount = 1;
//Are we currently connected?
boolean connected = false;

WiFiUDP udp;

Rx5808 nodes[] = {Rx5808(A0, D3, D2, D12), Rx5808(A1, D3, D2, D11), Rx5808(A3, D3, D2, D10), Rx5808(A4, D3, D2, D9)};

RssiFilter*** filters;
NodeReading*** readings;
int readingIndex = 0;


void WiFiEvent(WiFiEvent_t event){
    switch(event) {
      case ARDUINO_EVENT_WIFI_STA_GOT_IP:
          //When connected set 
          debugprint("WiFi connected! IP address: ");
          debugprintln(WiFi.localIP());  
          //initializes the UDP state
          //This initializes the transfer buffer
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

void initReadings(){
  readings = new NodeReading**[nodeCount];

  for(int i = 0; i < nodeCount; i++){
    readings[i] = new NodeReading*[readingBuffer];
    for(int j = 0; j < readingBuffer; j++){
      readings[i][j] = new NodeReading();
    }
  }
}
void initFilters(){
  filters = new RssiFilter**[nodeCount];
  for(int i =0; i < nodeCount; i++){
    filters[i] = new RssiFilter*[1];

    filters[i][0] = new MeanFilter(FirstMeanFilterSpan);
  }
}
void initRx58080Modules(){
  for(Rx5808 i : nodes){
    i.Init();
  }
}
void setup() {
  Serial.begin(57600);
  pinMode(LED_BUILTIN, OUTPUT);

  digitalWrite(LED_BUILTIN, HIGH);

  initReadings();
  initFilters();
  initRx58080Modules();

  //Connect to the WiFi network
  connectToWiFi(networkSid, networkPassword);
  
  digitalWrite(LED_BUILTIN, LOW);
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

void writeUdp(WiFiUDP* udp,int value){
  debugprint("Writing int: ");
  debugprintln(value);
  int segments = sizeof(int) / sizeof(uint8_t);
  for(int i = 0; i < segments; i ++){
    debugprint("Shift by: ");
    int shiftBy =(i * sizeof(uint8_t) * 8);
    debugprint(shiftBy);
    debugprint(" Shifted value: ");
    int shifted = value >> shiftBy;
    debugprintln(shifted);
    udp->write(shifted);
  }
  
  debugprint("Completed Writing int: ");
  debugprintln(value);
}
void writeUdp(WiFiUDP* udp,long value){
  int segments = sizeof(long) / sizeof(uint8_t);
  debugprint("Writing long: ");
  debugprintln(value);
  for(int i = 0; i < segments; i ++){
    debugprint("Shift by: ");
    int shiftBy =(i * sizeof(uint8_t) * 8);
    debugprint(shiftBy);
    debugprint(" Shifted value: ");
    int shifted = value >> shiftBy;
    debugprintln(shifted);
    udp->write(shifted);
  }
  
  debugprint("Completed Writing long: ");
  debugprintln(value);
}
void loop() {
  FlashLED();

  if(connected){
    int nodeReadIndex = 0;
    
    for(Rx5808 node : nodes){

      int rawReading = node.GetRssi();
      int filteredReading = rawReading;
      for(int i = 0; i< filterCount; i++){
        filteredReading = filters[nodeReadIndex][i]->GetValue(filteredReading);
      }
      
      readings[nodeReadIndex][readingIndex]->Rssi = filteredReading;
      readings[nodeReadIndex][readingIndex]->Tick = millis();
      nodeReadIndex ++;
    }
    readingIndex++;
    if(readingIndex >= readingBuffer){
        debugprintln("Sending Udp");
        udp.beginPacket(udpAddress,udpPort);
        writeUdp(&udp,nodeCount);
        writeUdp(&udp,readingBuffer);
        int nodeIndex = 0;
        for(int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex ++){  
          writeUdp(&udp,nodeIndex);
          for(int readingIndex = 0; readingIndex < readingBuffer; readingIndex++){
              NodeReading reading = *readings[nodeIndex][readingIndex];
              writeUdp(&udp,reading.Tick);
              writeUdp(&udp,reading.Rssi);
          }
        }
        udp.endPacket();

        readingIndex = 0;
        debugprintln("UDP sent");
    }
    debugprint("Reading index: ");
    debugprintln(readingIndex);
  }
}



//wifi event handler
