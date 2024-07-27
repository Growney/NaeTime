
#define ALIVE_COMMAND 1
#define RSSI_TICK_COMMAND 2
#define CROSSING_ENTERED_COMMAND 3
#define CROSSING_LEFT_COMMAND 4

struct PacketHeader
{
    char StartByte = 0x5a;
    char CommandType;
    int DataLength;
    char* Data;
};

struct NodeRssi{
    int NodeIndex;
    long Tick;
    long Rssi;
};

struct NodeEnteredCrossing{
    int NodeIndex;
    long Tick;
    int CrossingCount;
};

struct NodeLeftCrossing{
    int NodeIndex;
    long Tick;
    int CrossingCount;
};

struct Alive{
    long Tick;
};