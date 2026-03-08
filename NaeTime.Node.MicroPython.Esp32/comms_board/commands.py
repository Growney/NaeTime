class TuneLane:
    def __init__(self,lane,bandId,frequency_in_mhz):
        self._lane = lane
        self._bandId = bandId
        self._frequency_in_mhz = frequency_in_mhz
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def bandId(self):
        return self._bandId

    @property
    def frequency_in_mhz(self):
        return self._frequency_in_mhz

class ConfigureLaneEnabled:
    def __init__(self,lane,enabled):
        self._lane = lane
        self._enabled = enabled
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def enabled(self):
        return self._enabled

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

class NodeStatus:
    def __init__(self, current_time, lane_count, enabled_lanes, lane_statuses):
        self._current_time = current_time
        self._lane_count = lane_count
        self._enabled_lanes = enabled_lanes
        self._lane_statuses = lane_statuses

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
    def lane_statuses(self):
        return self._lane_statuses

class LaneRssi:
    def __init__(self,lane,rssi_read_time,rssi):
        self._lane = lane
        self._rssi_read_time = rssi_read_time
        self._rssi = rssi
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def rssi_read_time(self):
        return self._rssi_read_time

    @property
    def rssi(self):
        return self._rssi

class LaneConfiguration:
    def __init__(self,bandId,frequency_in_mhz,entry_threshold,exit_threshold):
        self._bandId = bandId   
        self._frequency_in_mhz = frequency_in_mhz
        self._entry_threshold = entry_threshold
        self._exit_threshold = exit_threshold
    
    @property
    def bandId(self):
        return self._bandId

    @property
    def frequency_in_mhz(self):
        return self._frequency_in_mhz
    
    @property
    def entry_threshold(self):
        return self._entry_threshold
    
    @property
    def exit_threshold(self):
        return self._exit_threshold

class RequestLaneConfigurations:
    def __init__(self, lanes):
        self._lanes = lanes
    
    @property
    def lanes(self):
        return self._lanes
    
class LaneConfigurationsResponse():
    def __init__(self,enabled_lanes, lane_configurations):
        self._enabled_lanes = enabled_lanes
        self._lane_configurations = lane_configurations
    
    @property
    def enabled_lanes(self):
        return self._enabled_lanes

    @property
    def lane_configurations(self):
        return self._lane_configurations

class LanePassEvent:
    def __init__(self,lane,pass_count,start_time,end_time):
        self._lane = lane
        self._pass_count = pass_count
        self._start_time = start_time
        self._end_time = end_time
    
    @property
    def lane(self):
        return self._lane
    
    @property
    def pass_count(self):
        return self._pass_count
    
    @property
    def start_time(self):
        return self._start_time
    
    @property
    def end_time(self):
        return self._end_time

# ---------------------------------------------------------------------------
# UI-board ↔ comms-board UART protocol commands
# ---------------------------------------------------------------------------

class QueryNetworkStatus:
    """Sent by the UI board to request full network status from the comms board.
    Payload is empty."""
    pass

class NetworkStatusResponse:
    """Sent by the comms board in reply to QueryNetworkStatus.
    Carries connection status, DHCP flag, IP, subnet, and gateway (IPv4 only)."""
    def __init__(self, connected, dhcp, ip, subnet, gateway):
        self._connected = connected
        self._dhcp = dhcp
        self._ip = ip
        self._subnet = subnet
        self._gateway = gateway

    @property
    def connected(self):
        return self._connected

    @property
    def dhcp(self):
        return self._dhcp

    @property
    def ip(self):
        return self._ip

    @property
    def subnet(self):
        return self._subnet

    @property
    def gateway(self):
        return self._gateway

class ConfigureNetwork:
    """Sent by the UI board to apply a new IPv4 network configuration.
    dhcp=True: enable DHCP (ip/subnet/gateway are ignored by the comms board).
    dhcp=False: static config with the provided ip, subnet and gateway strings."""
    def __init__(self, dhcp, ip, subnet, gateway):
        self._dhcp = dhcp
        self._ip = ip
        self._subnet = subnet
        self._gateway = gateway

    @property
    def dhcp(self):
        return self._dhcp

    @property
    def ip(self):
        return self._ip

    @property
    def subnet(self):
        return self._subnet

    @property
    def gateway(self):
        return self._gateway

