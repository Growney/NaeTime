#include "MeanFilter.h"
#define DefaultSpan 10;

MeanFilter::MeanFilter(int span){
    _span = span;
    _buffer = new int[_span];
}
MeanFilter::MeanFilter(){
    _span = DefaultSpan;
    _buffer = new int[_span];
}

int MeanFilter::GetValue(int nextValue){
    _buffer[_currentIndex] = nextValue;
    _currentIndex = (_currentIndex + 1)  % _span;

    if(_currentIndex == 0){
        _shouldAverage = true;
    }

    if(!_shouldAverage){
        return nextValue;
    }

    int sum = 0;
    for(int i = 0; i < _span; i ++){
        sum = sum + _buffer[i];
    }

    return sum / _span;

}