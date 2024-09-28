#ifndef RSSIFILTER_H
#define RSSIFILTER_H

class RssiFilter{
    
    public:
        virtual int GetValue(int nextValue);
};
#endif