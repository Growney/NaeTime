#include <stdlib.h>
#include "RssiFilter.h"

class MeanFilter: public RssiFilter
{
    private:
        int _span;
        int* _buffer;
        int _currentIndex = 0;
        bool _shouldAverage = false;

    public:
        int GetValue(int nextValue);
        MeanFilter(int span);
        MeanFilter();
        ~MeanFilter(){
            delete _buffer;
        }
};