import machine
from devices import filters

class ADCReader:
    def __init__(self, pin, sample_rate, cutoff_frequency):
        self.pin = pin
        self.adc = machine.ADC(machine.Pin(pin))
        self.adc.atten(machine.ADC.ATTN_11DB)
        self.adc.width(machine.ADC.WIDTH_12BIT)
        self.filter = filters.LowPassFilter(cutoff_frequency, sample_rate)
    
    def read_value(self):
        readValue = self.adc.read()
        raw_value = self.adc.read_u16()
        filtered = self.filter.get_value(raw_value)
        voltage = self.adc.read_uv()
        return filtered


class Peak:
    def __init__(self, start, end):
        self.start = start
        self.end = end

    def __repr__(self):
        return f"Peak(start={self.start}, end={self.end})"

class PeakDetector:
    def __init__(self, entry_threshold, exit_threshold):
        """
        Initialize the Peak Detector.

        :param entry_threshold: The threshold to detect the start of a peak.
        :param exit_threshold: The threshold to detect the end of a peak.
        """
        self._entry_threshold = entry_threshold
        self._exit_threshold = exit_threshold
        self.in_peak = False
        self.peak_start_time = None

    @property 
    def entry_threshold(self):
        return self._entry_threshold
    
    @entry_threshold.setter
    def entry_threshold(self, value):
        self._entry_threshold = value
    
    @property
    def exit_threshold(self):
        return self._exit_threshold
    
    @exit_threshold.setter
    def exit_threshold(self, value):
        self._exit_threshold = value
    
    def add_reading(self, rssi, time):
        """
        Add an RSSI reading and check for peaks.

        :param rssi: The current RSSI reading.
        :param time: The time of the RSSI reading.
        :return: 0 if outside a peak, 1 if a peak is entered, 2 if inside a peak, 3 if a value leaves a peak.
        """

        if not self.in_peak and rssi > self._entry_threshold:
            self.in_peak = True
            self.peak_start_time = time
            return 1

        if self.in_peak:
            if rssi < self._exit_threshold:
                self.in_peak = False
                return 3
            else:
                return 2
            
        return 0