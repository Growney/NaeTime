#include "NetworkCommunication.h"


void NetworkCommunication::Init(const char *deviceId,int deviceIdLength){
    WiFi.mode(WIFI_STA);
    deviceIdParameter->setValue(deviceId,deviceIdLength);

    wifiManager.addParameter(deviceIdParameter);
    wifiManager.setConfigPortalBlocking(false);
    wifiManager.autoConnect("NaeTime Node");
}
void NetworkCommunication::Tick(){
    
    wifiManager.process();
    if(IsConnectedToNetwork()){
        if(!wifiConnectedLastTick){
            server->begin();
            wifiConnectedLastTick = true;
        }

        if(!client.connected()){
            client = server->available();
        }
    }
}
bool NetworkCommunication::IsConnectedToNetwork(){
    return WiFi.status() == WL_CONNECTED;
}

bool NetworkCommunication::IsConnectedToClient(){
    return WiFi.status() == WL_CONNECTED && client.connected();
}

void NetworkCommunication::TransmitAlive(long current){
    client.write(START_BYTE);
    client.write(ALIVE_COMMAND);
    client.write((char*)current);
}
void NetworkCommunication::TransmitNodeRssi(int nodeIndex, long tick, int rssi){
    client.write(START_BYTE);
    client.write(RSSI_TICK_COMMAND);
    client.write((char*)nodeIndex);
    client.write((char*)tick);
    client.write((char*)rssi);
}
void NetworkCommunication::TransmitNodeEnteredCrossing(int nodeIndex, long tick, int rssi){
    client.write(START_BYTE);
    client.write(CROSSING_ENTERED_COMMAND);
    client.write((char*)nodeIndex);
    client.write((char*)tick);
    client.write((char*)rssi);
}
void NetworkCommunication::TransmitNodeLeftCrossing(int nodeIndex, long tick, int rssi){
    client.write(START_BYTE);
    client.write(CROSSING_LEFT_COMMAND);
    client.write((char*)nodeIndex);
    client.write((char*)tick);
    client.write((char*)rssi);
}

void NetworkCommunication::BroadcastDeviceInfo(long current){
    String deviceId = deviceIdParameter->getValue();
    IPAddress ipAddress = WiFi.localIP();
    String message =  String(current) + "," +  ipAddress.toString() + "," + deviceId;
    Serial.println(message);
    udp.broadcastTo(message.c_str(),BROADCAST_PORT);
}