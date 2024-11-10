import machine
from devices import filters

class ADCReader:
    def __init__(self, pin, filter_size=10):
        self.adc = machine.ADC(machine.Pin(pin))
        self.filter = filters.MeanFilter(filter_size)
    
    def read_value(self):
        raw_value = self.adc.read_u16()
        return self.filter.get_value(raw_value)