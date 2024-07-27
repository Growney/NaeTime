#include "RssiTimerNode.h"
#include "Rx5808.h"
#include "../filters/RssiFilter.h"
#include "../filters/MeanFilter.h"

#define PrimaryMeanFilterSpan 250

class Rx5808RssiTimerNode: public RssiTimerNode{

    private: 
        Rx5808* receiver;
        RssiFilter filters[1] = {MeanFilter(PrimaryMeanFilterSpan)};
        void UpdateState(int filteredValued);

    public:
        Rx5808RssiTimerNode(int rssiPin, int clockPin, int dataPin, int selectPin);
        ~Rx5808RssiTimerNode(){
            delete receiver;
            delete [] filters;
        }
        void Init();
        void TickRssi();
        bool Tune(int frequencyInMhz);
        RssiNodeState GetCurrentState();

};