class TuneLane:
    def __init__(self,lane,frequency_in_mhz):
        self._lane = lane
        self._frequency_in_mhz = frequency_in_mhz
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def frequency_in_mhz(self):
        return self._frequency_in_mhz

class ConfigureLaneEntryThreshold:
    def __init__(self,lane,entry_threshold):
        self._lane = lane
        self._entry_threshold = entry_threshold
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def entry_threshold(self):
        return self._entry_threshold

class ConfigureLaneExitThreshold:
    def __init__(self,lane,exit_threshold):
        self._lane = lane
        self._exit_threshold = exit_threshold
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def exit_threshold(self):
        return self._exit_threshold

class ConfigureNode:
    def __init__(self, node_id, transmit_frequency_hz, polling_frequency_hz):
        self._node_id = node_id
        self._transmit_frequency_hz = transmit_frequency_hz
        self._polling_frequency_hz = polling_frequency_hz

    @property
    def node_id(self):
        return self._node_id
    
    @property
    def transmit_frequency_hz(self):
        return self._transmit_frequency_hz
    
    @property
    def polling_frequency_hz(self):
        return self._polling_frequency_hz

class NodeTimings:
    def __init__(self, current_time, lane_count, enabled_lanes, lane_timings):
        self._current_time = current_time
        self._lane_count = lane_count
        self._enabled_lanes = enabled_lanes
        self._lane_timings = lane_timings

    @property
    def current_time(self):
        return self._current_time
    
    @property
    def lane_count(self):
        return self._lane_count
    
    @property
    def enabled_lanes(self):
        return self._enabled_lanes
    
    @property
    def lane_timings(self):
        return self._lane_timings


class LaneTimings:
    def __init__(self,rssi, last_pass, pass_count):
        self._rssi = rssi
        self._last_pass = last_pass
        self._pass_count = pass_count
    
    @property
    def rssi(self):
        return self._rssi
   
    @property
    def pass_count(self):
        return self._pass_count
    
    @property
    def last_pass(self):
        return self._last_pass



    
