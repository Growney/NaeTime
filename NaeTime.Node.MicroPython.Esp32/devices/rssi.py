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


class PeakDetector:
    def __init__(self, min_values=50):
        """
        Initialize the Peak Detector.

        :param min_values: The minimum number of values before detecting peaks.
        """
        self.min_values = min_values
        self.previous_rssi = None
        self.peaks = []
        self.rssi_readings = []
        self.sum_rssi = 0
        self.sum_rssi_squared = 0

    def add_reading(self, rssi):
        """
        Add an RSSI reading and check for peaks.

        :param rssi: The current RSSI reading.
        :return: True if a peak is detected, False otherwise.
        """
        self.rssi_readings.append(rssi)
        self.sum_rssi += rssi
        self.sum_rssi_squared += rssi ** 2

        if len(self.rssi_readings) < self.min_values:
            self.previous_rssi = rssi
            return False

        threshold = self.calculate_dynamic_threshold()

        if self.previous_rssi is None:
            self.previous_rssi = rssi
            return False

        if rssi > self.previous_rssi + threshold:
            self.peaks.append(rssi)
            self.previous_rssi = rssi
            return True

        self.previous_rssi = rssi
        return False

    def calculate_dynamic_threshold(self):
        """
        Calculate the dynamic threshold based on the standard deviation of the RSSI readings.

        :return: The dynamic threshold.
        """
        n = len(self.rssi_readings)
        mean = self.sum_rssi / n
        variance = (self.sum_rssi_squared / n) - (mean ** 2)
        return math.sqrt(variance)

    def get_peaks(self):
        """
        Get the list of detected peaks and clear the peaks list.

        :return: List of detected peaks.
        """
        peaks = self.peaks[:]
        self.peaks.clear()
        return peaks