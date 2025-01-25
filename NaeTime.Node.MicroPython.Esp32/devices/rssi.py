import machine
import math
from devices import filters

class ADCReader:
    def __init__(self, pin, filter_size=50):
        self.adc = machine.ADC(machine.Pin(pin))
        self.filter = filters.MeanFilter(filter_size)
    
    def read_value(self):
        raw_value = self.adc.read_u16()
        return self.filter.get_value(raw_value)


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
        self.entry_threshold = entry_threshold
        self.exit_threshold = exit_threshold
        self.previous_rssi = None
        self.in_peak = False
        self.peak_start_time = None

    def add_reading(self, rssi, time):
        """
        Add an RSSI reading and check for peaks.

        :param rssi: The current RSSI reading.
        :param time: The time of the RSSI reading.
        :return: 0 if outside a peak, 1 if a peak is entered, 2 if inside a peak, 3 if a value leaves a peak.
        """

        if self.previous_rssi is None:
            self.previous_rssi = rssi
            return 0

        if not self.in_peak and rssi > self.previous_rssi + self.entry_threshold:
            self.in_peak = True
            self.peak_start_time = time
            self.previous_rssi = rssi
            return 1

        if self.in_peak:
            if rssi < self.previous_rssi - self.exit_threshold:
                self.in_peak = False
                self.previous_rssi = rssi
                return 3
            else:
                self.previous_rssi = rssi
                return 2

        self.previous_rssi = rssi
        return 0