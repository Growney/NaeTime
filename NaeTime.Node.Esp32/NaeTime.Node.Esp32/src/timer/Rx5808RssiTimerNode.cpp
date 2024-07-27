#include <Arduino.h>
#include "../timer/Rx5808RssiTimerNode.h"
#include "../filters/MeanFilter.h"
#include "../debug.h"


Rx5808RssiTimerNode::Rx5808RssiTimerNode(int rssiPin, int clockPin, int dataPin, int selectPin){
    receiver = new Rx5808(rssiPin, clockPin, dataPin, selectPin);
}

void Rx5808RssiTimerNode::Init(){
    receiver->Init();
}
RssiNodeState Rx5808RssiTimerNode::GetCurrentState(){
    return currentState;
}
void Rx5808RssiTimerNode::TickRssi(){

    long currentTick = micros();
    int rawValue = receiver->GetRssi();

    int filteredValue = rawValue;
    for(RssiFilter filter : filters){
        filteredValue = filter.GetValue(filteredValue);
    }

    UpdateState(filteredValue);
    if(currentState.CurrentState == ENTERED_CROSSING){
        currentState.LastCrossingStart = currentTick;
        currentState.LastCrossingEnd = 0;
    } else if(currentState.CurrentState == LEFT_CROSSING){
        currentState.LastCrossingEnd = currentTick;
    }
    currentState.LastTick = currentTick;
    currentState.DidFilteredRssiChangeLastTick = currentState.LastFilteredRssi == filteredValue;
    currentState.LastFilteredRssi = filteredValue;
    currentState.LastRssi = rawValue;
    
}

void Rx5808RssiTimerNode::UpdateState(int filteredValue){

    if(currentState.CurrentState == BELOW_TRANSMISSION_THRESHOLD){
        if(filteredValue > TransmissionThreshold){
            currentState.CurrentState = ABOVE_TRANSMISSION_THRESHOLD;
        }
        if(filteredValue > EntryThreshold){
            currentState.CurrentState = ENTERED_CROSSING;
        }
    }
    else if(currentState.CurrentState == ABOVE_TRANSMISSION_THRESHOLD){
        if(filteredValue < TransmissionThreshold){
            currentState.CurrentState = BELOW_TRANSMISSION_THRESHOLD;
        }
        
        if(filteredValue > EntryThreshold){
            currentState.CurrentState = ENTERED_CROSSING;
        }
    }
    else if(currentState.CurrentState == ENTERED_CROSSING){
        if(filteredValue < ExitThreshold){
            currentState.CurrentState = LEFT_CROSSING;
        }
        else{
            currentState.CurrentState = CROSSING;
        }
    }
    else if(currentState.CurrentState == CROSSING){
        if(filteredValue < ExitThreshold){
            currentState.CurrentState = LEFT_CROSSING;
        }
    }
    else if(currentState.CurrentState == LEFT_CROSSING){
        if(filteredValue > EntryThreshold){
            currentState.CurrentState = ENTERED_CROSSING;
        }else{
            currentState.CurrentState = ABOVE_TRANSMISSION_THRESHOLD;
        }
    }
}

bool Rx5808RssiTimerNode::Tune(int frequencyInMhz){
    requestedFrequency = frequencyInMhz;
    receiver->SetFrequency(frequencyInMhz);
    actualFrequency = receiver->GetActualStoredFrequency();

    return receiver->ConfirmFrequency(frequencyInMhz);
}