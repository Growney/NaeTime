#ifndef RSSITIMERNODE_H
#define RSSITIMERNODE_H
 
#define BELOW_TRANSMISSION_THRESHOLD 0
#define ABOVE_TRANSMISSION_THRESHOLD 1
#define ENTERED_CROSSING 2
#define CROSSING 3
#define LEFT_CROSSING 4

struct RssiNodeState{
    public:
        int CurrentState;
        long LastCrossingStart;
        long LastCrossingEnd;
        int LastRssi;
        int LastFilteredRssi;
        bool DidFilteredRssiChangeLastTick;
        long LastTick;
        long LastTransmittedTick;
};

class RssiTimerNode{

    protected:
        int requestedFrequency;
        int actualFrequency;
        RssiNodeState currentState;
        

    public:
        int EntryThreshold = 1000;
        int ExitThreshold = 1000;
        int TransmissionThreshold = 500;
        
        int RequestedFrequency() const { return requestedFrequency; }
        int ActualFrequency() const { return actualFrequency; }
        virtual void Init(){

        }
        virtual void Tune(int frequencyInMhz)
        {
            requestedFrequency = frequencyInMhz;
            actualFrequency = frequencyInMhz;
        }
        virtual void TickRssi(){

        }
        virtual RssiNodeState GetCurrentState();
        virtual void MarkTickAsTrasmitted()
        {
            currentState.LastTransmittedTick = currentState.LastTick;
        }
};
#endif

