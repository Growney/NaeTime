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

class LaneTimings:
    def __init__(self,current_time, lane, rssi, last_pass, pass_count):
        self._current_time = current_time
        self._lane = lane
        self._rssi = rssi
        self._last_pass = last_pass
        self._pass_count = pass_count

    @property
    def current_time(self):
        return self._current_time

    @property
    def lane(self):
        return self._lane
    
    @property
    def rssi(self):
        return self._rssi
    
    @property
    def last_pass(self):
        return self._last_pass
    
    @property
    def pass_count(self):
        return self._pass_count
    

    
