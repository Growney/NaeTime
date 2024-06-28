#include <Arduino.h>
#include "Rx5808.h"
#include "NodeReading.h"

#include <WiFi.h>
#include <WiFiUdp.h>

const char * networkSid = "Beagle WIFI";
const char * networkPassword = "whatcanthebeaglesmellunderthetree";

const char * udpAddress = "192.168.0.124";
const int udpPort = 11000;

long lastLedChange = 0;
bool ledOn = false;
const int nodeCount = 4;
const int readingBuffer = 100;
//Are we currently connected?
boolean connected = false;

WiFiUDP udp;

Rx5808 nodes[] = {Rx5808(A0, D3, D2, D12), Rx5808(A1, D3, D2, D11), Rx5808(A3, D3, D2, D10), Rx5808(A4, D3, D2, D9)};


NodeReading*** readings;
int readingIndex = 0;

void WiFiEvent(WiFiEvent_t event){
    switch(event) {
      case ARDUINO_EVENT_WIFI_STA_GOT_IP:
          //When connected set 
          Serial.print("WiFi connected! IP address: ");
          Serial.println(WiFi.localIP());  
          //initializes the UDP state
          //This initializes the transfer buffer
          connected = true;
          break;
      case ARDUINO_EVENT_WIFI_STA_DISCONNECTED:
          Serial.println("WiFi lost connection");
          connected = false;
          break;
      default: break;
    }
}
void connectToWiFi(const char * ssid, const char * pwd){
  Serial.println("Connecting to WiFi network: " + String(ssid));

  // delete old config
  WiFi.disconnect(true);
  //register event handler
  WiFi.onEvent(WiFiEvent);
  
  //Initiate connection
  WiFi.begin(ssid, pwd);

  Serial.println("Waiting for WIFI connection...");
}

void setup() {

  delay(5000);
  Serial.begin(57600);
  
  pinMode(LED_BUILTIN, OUTPUT);

  Serial.println("Before allocation");
  readings = new NodeReading**[nodeCount];

  for(int i = 0; i < nodeCount; i++){
    readings[i] = new NodeReading*[readingBuffer];
    for(int j = 0; j < readingBuffer; j++){
      readings[i][j] = new NodeReading();
    }
  }
  
  Serial.println("After allocation");
  

  for(Rx5808 i : nodes){
    i.Init();
  }
  //Connect to the WiFi network
  connectToWiFi(networkSid, networkPassword);
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

void loop() {
  FlashLED();

  if(connected){
    int nodeReadIndex = 0;
    
  Serial.println("before reading");
    for(Rx5808 node : nodes){

      readings[nodeReadIndex][readingIndex]->Rssi = node.GetRssi();
      readings[nodeReadIndex][readingIndex]->Tick = micros();
      nodeReadIndex ++;
    }
    
  Serial.println("after reading");
    readingIndex++;
    if(readingIndex >= readingBuffer){
        udp.beginPacket(udpAddress,udpPort);
        udp.print(nodeCount);
        udp.print(",");
        udp.print(readingBuffer);
        udp.print(",");
        int nodeIndex = 0;
        for(int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex ++){
          udp.print(nodeIndex);
          udp.print(",");
          for(int readingIndex = 0; readingIndex < readingBuffer; readingIndex++){
              if(readingIndex > 0){
                udp.print(",");
              }
              NodeReading reading = *readings[nodeIndex][readingIndex];

              udp.print(reading.Tick);
              udp.print(",");
              udp.print(reading.Rssi);
          }
        }
        udp.endPacket();

        readingIndex = 0;
    }
    Serial.print("Reading index: ");
    Serial.println(readingIndex);
  }
}


//wifi event handler
