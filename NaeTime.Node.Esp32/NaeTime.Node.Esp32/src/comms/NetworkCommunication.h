
#include <WiFi.h>
#include <WiFiUdp.h>
#include <WiFiManager.h>
#include "AsyncUDP.h"
#include <FastCRC.h>

#define SERVER_PORT 11000
#define BROADCAST_PORT 11001

#define START_BYTE 0x5a

#define ALIVE_COMMAND 1
#define RSSI_TICK_COMMAND 2
#define CROSSING_ENTERED_COMMAND 3
#define CROSSING_LEFT_COMMAND 4

class NetworkCommunication{

    private:
        WiFiManager wifiManager;
        WiFiManagerParameter* deviceIdParameter;
        WiFiServer* server;
        WiFiClient client;
        AsyncUDP udp;
        bool wifiConnectedLastTick;

    public:
        NetworkCommunication(){
            deviceIdParameter = new WiFiManagerParameter("deviceId","Device Id","",20);
            server = new WiFiServer(SERVER_PORT);
        }
        void Init(const char *deviceId,int deviceIdLength);
        void Tick();
        bool IsConnectedToNetwork();
        bool IsConnectedToClient();

        void TransmitAlive(long current);
        void TransmitNodeRssi(int nodeIndex, long tick, int rssi);
        void TransmitNodeEnteredCrossing(int nodeIndex, long tick, int rssi);
        void TransmitNodeLeftCrossing(int nodeIndex, long tick, int rssi);
        void BroadcastDeviceInfo(long current);
};