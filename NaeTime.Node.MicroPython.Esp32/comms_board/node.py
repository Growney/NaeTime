
import time

from devices.rssi import ADCReader
from devices.rssi import PeakDetector
from devices.rx5808 import Rx5808RegisterCommunication

class LaneConfiguration:
    def __init__(self,is_enabled,frequency_in_mhz,entry_threshold,exit_threshold):
        self._is_enabled = is_enabled
        self._frequency_in_mhz = frequency_in_mhz
        self._entry_threshold = entry_threshold
        self._exit_threshold = exit_threshold
    
    @property
    def frequency_in_mhz(self):
        return self._frequency_in_mhz
    
    @property
    def entry_threshold(self):
        return self._entry_threshold
    
    @property
    def exit_threshold(self):
        return self._exit_threshold

class NodeLane:
    def __init__(self, rx_module, adc_reader, frequency_in_mhz, is_enabled, entry_threshold, exit_threshold):
        self._frequency_in_mhz = frequency_in_mhz
        self._is_enabled = is_enabled

        self._rx_module = rx_module
        self._adc_reader = adc_reader
        self._peak_detector = PeakDetector(entry_threshold, exit_threshold)

        self._rssi = 0
        self._last_pass = 0
        self._current_pass_maximum = 0
        self._current_pass_maximum_time = 0
        self._pass_count = 0
        self._last_reading = 0
    
    @property
    def rssi(self):
        return self._rssi
    
    @property
    def last_pass(self):
        return self._last_pass
    
    @property
    def pass_count(self):
        return self._pass_count

    @property
    def last_reading(self):
        return self._last_reading

    def take_reading(self):
        current_time = time.ticks_ms()
        current_rssi = self._adc_reader.read_value()
        self._last_reading = current_time
        
        pass_state = self._peak_detector.add_reading(current_rssi,current_time)

        # Passing the entry threshold
        if(pass_state == 1):
            self._current_pass_maximum = current_rssi
            self._current_pass_maximum_time = current_time
        # Inside the threshold
        elif(pass_state == 2):
            if(current_rssi > self._current_pass_maximum):
                self._current_pass_maximum = current_rssi
                self._current_pass_maximum_time = current_time
        # Leaving the threshold
        elif(pass_state == 3):
            self._last_pass = self._current_pass_maximum_time
            self._current_pass_maximum = 0
            self._current_pass_maximum_time = 0
            self._pass_count += 1

class NodeConfiguration:
    def __init__(self,transmit_delay_ms, polling_delay_ms, filter_cutoff_frequency, lane_configurations):
        self._lane_configurations = lane_configurations
        self._transmit_delay_ms = transmit_delay_ms
        self._polling_delay_ms = polling_delay_ms
        self._filter_cutoff_frequency = filter_cutoff_frequency
    
    @property
    def transmit_delay_ms(self):
        return self._transmit_delay_ms
    
    @property
    def polling_delay_ms(self):
        return self._polling_delay_ms
    
    @property
    def filter_cutoff_frequency(self):
        return self._filter_cutoff_frequency

    @property
    def lane_configurations(self):
        return self._lane_configurations
    

        