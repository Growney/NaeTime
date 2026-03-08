# uart_comms.py
# Synchronous client-side handler for the UI-board <-> comms-board UART protocol.
#
# Framing is identical to the comms_board NodeCommunication:
#   [PACKET_START] [cmd_id:1B] [crc:2B] [length:2B] [payload...] [PACKET_END]
# Special bytes inside the payload are escaped with ESCAPE_BYTE.
#
# Command IDs used by this protocol (must match comms_board/comms.py):
#   QUERY_NETWORK_STATUS    = 0x20  (UI -> comms, empty payload)
#   NETWORK_STATUS_RESPONSE = 0x21  (comms -> UI, payload: connected(1B) dhcp(1B) ip(4B) subnet(4B) gateway(4B))
#   CONFIGURE_NETWORK       = 0x22  (UI -> comms, payload: dhcp(1B) ip(4B) subnet(4B) gateway(4B))
#   ACK                     = 0xFF  (comms -> UI, inner payload starts with original cmd_id)
#   ERROR                   = 0xFE  (comms -> UI, inner payload starts with original cmd_id)

import struct
import time

# ---------------------------------------------------------------------------
# Protocol constants
# ---------------------------------------------------------------------------
QUERY_NETWORK_STATUS    = 0x20
NETWORK_STATUS_RESPONSE = 0x21
CONFIGURE_NETWORK       = 0x22
REQUEST_LANE_CONFIGURATIONS = 0x07
TUNE_LANE               = 0x01

RESPONSE = 0xFC
STATUS = 0xFD
ERROR = 0xFE
ACK = 0xFF

PACKET_START = 0x5A
PACKET_END   = 0x5B
ESCAPE_BYTE  = 0x5C
ESCAPE_ADDER = 0x40


# ---------------------------------------------------------------------------
# CRC-16 (identical to comms_board/comms.py CRC16 class)
# ---------------------------------------------------------------------------
class CRC16:
    def __init__(self, polynomial=0x8005, initial_value=0xFFFF):
        self.polynomial = polynomial
        self.initial_value = initial_value
        self.table = self._create_table()

    def _create_table(self):
        table = []
        for byte in range(256):
            crc = 0
            for _ in range(8):
                if (byte ^ crc) & 0x01:
                    crc = (crc >> 1) ^ self.polynomial
                else:
                    crc >>= 1
                byte >>= 1
            table.append(crc)
        return table

    def reflect(self, data, width):
        reflection = 0
        for i in range(width):
            if data & (1 << i):
                reflection |= 1 << (width - 1 - i)
        return reflection

    def calculate(self, data):
        crc = self.initial_value
        for byte in data:
            byte = self.reflect(byte, 8)
            crc = (crc >> 8) ^ self.table[(crc ^ byte) & 0xFF]
        crc = self.reflect(crc, 16)
        return crc


# ---------------------------------------------------------------------------
# Packet builder / parser helpers
# ---------------------------------------------------------------------------

def _create_packet_payload(crc16, command_id, payload):
    """Build the raw (pre-escape) framing bytes for a packet."""
    header = struct.pack("<BHH", command_id, 0, len(payload))
    crc = crc16.calculate(header + payload)
    header = struct.pack("<BHH", command_id, crc, len(payload))
    return header + payload


def _escape_packet(raw):
    """Wrap raw bytes in START/END, escaping special bytes."""
    result = bytes([PACKET_START])
    for byte in raw:
        if byte in (PACKET_START, PACKET_END, ESCAPE_BYTE, 0x03):
            result += bytes([ESCAPE_BYTE, byte + ESCAPE_ADDER])
        else:
            result += bytes([byte])
    result += bytes([PACKET_END])
    return result


def _build_packet(crc16, command_id, payload=b""):
    raw = _create_packet_payload(crc16, command_id, payload)
    return _escape_packet(raw)


def _check_payload_crc(crc16, command_id, received_crc, length, payload):
    crc_data = struct.pack("<BHH", command_id, 0, length) + payload
    return received_crc == crc16.calculate(crc_data)


# ---------------------------------------------------------------------------
# Synchronous UART protocol client
# ---------------------------------------------------------------------------

class UARTCommsClient:
    """Synchronous UART client for the UI board.

    Usage:
        client = UARTCommsClient(comms_uart)
        connected = client.query_network_connected()   # True / False / None
    """

    def __init__(self, uart):
        self._uart = uart
        self._crc = CRC16()
        self._rx_buffer = []

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def query_network_status(self, timeout_ms=10000):
        """Ask the comms board for its full IPv4 network status.

        Returns a dict with keys:
            'connected' (bool), 'dhcp' (bool),
            'ip' (str), 'subnet' (str), 'gateway' (str)
        or None if no response arrives within timeout_ms.
        """
        packet = _build_packet(self._crc, QUERY_NETWORK_STATUS, b"")
        self._uart.write(packet)
        print("uart_comms: sent data: ", [hex(b) for b in packet])

        deadline = time.ticks_add(time.ticks_ms(), timeout_ms)
        self._rx_buffer = []

        while time.ticks_diff(deadline, time.ticks_ms()) > 0:
            waiting = self._uart.any()
            if waiting > 0:
                data = self._uart.read(waiting)
                if data:
                    print("uart_comms: received", len(data), "bytes:", [hex(b) for b in data])
                    for byte in data:
                        result = self._process_byte(byte)
                        if result is not None and result.get('type') == 'network_status':
                            return result['data']
            time.sleep_ms(5)

        print("uart_comms: timed out waiting for response")
        return None

    def configure_network(self, dhcp, ip, subnet, gateway, timeout_ms=10000):
        """Send a new IPv4 network configuration to the comms board.

        Args:
            dhcp (bool):    True to enable DHCP; False for static.
            ip (str):       Static IP address string, e.g. '192.168.1.4'.
            subnet (str):   Subnet mask string, e.g. '255.255.255.0'.
            gateway (str):  Gateway address string, e.g. '192.168.1.1'.
            timeout_ms:     How long to wait for an ACK/ERROR (default 10 s).

        Returns:
            True   – comms board acknowledged the new config
            False  – comms board returned an error
            None   – no response within timeout_ms
        """
        ip_bytes      = bytes(int(x) for x in ip.split('.'))
        subnet_bytes  = bytes(int(x) for x in subnet.split('.'))
        gateway_bytes = bytes(int(x) for x in gateway.split('.'))
        payload = struct.pack("<B4s4s4s", 1 if dhcp else 0, ip_bytes, subnet_bytes, gateway_bytes)
        packet = _build_packet(self._crc, CONFIGURE_NETWORK, payload)
        self._uart.write(packet)
        print("uart_comms: sent configure_network:", [hex(b) for b in packet])

        deadline = time.ticks_add(time.ticks_ms(), timeout_ms)
        self._rx_buffer = []

        while time.ticks_diff(deadline, time.ticks_ms()) > 0:
            waiting = self._uart.any()
            if waiting > 0:
                data = self._uart.read(waiting)
                if data:
                    print("uart_comms: received", len(data), "bytes:", [hex(b) for b in data])
                    for byte in data:
                        result = self._process_byte(byte)
                        if result is not None:
                            t = result.get('type')
                            if result.get('cmd') == CONFIGURE_NETWORK:
                                if t == 'ack':
                                    return True
                                elif t == 'error':
                                    return False
            time.sleep_ms(5)

        print("uart_comms: timed out waiting for configure_network response")
        return None

    def query_lane_configurations(self, lane_mask=0xFF, timeout_ms=10000):
        """Ask the comms board for lane radio configurations.

        Args:
            lane_mask:  Bitmask of lanes to query. Bit 0 = lane 0, bit 1 = lane 1,
                        etc.  0xFF requests all 8 lanes (default).
            timeout_ms: Response timeout in milliseconds (default 10 s).

        Returns a dict with keys:
            'enabled_lanes' (int bitmask),
            'lanes' (list of dicts with 'band_id', 'frequency_mhz',
                     'entry_threshold', 'exit_threshold')
        or None if no response arrives within timeout_ms.
        """
        payload = struct.pack("<B", lane_mask)
        packet = _build_packet(self._crc, REQUEST_LANE_CONFIGURATIONS, payload)
        self._uart.write(packet)
        print("uart_comms: sent request_lane_configurations:", [hex(b) for b in packet])

        deadline = time.ticks_add(time.ticks_ms(), timeout_ms)
        self._rx_buffer = []

        while time.ticks_diff(deadline, time.ticks_ms()) > 0:
            waiting = self._uart.any()
            if waiting > 0:
                data = self._uart.read(waiting)
                if data:
                    print("uart_comms: received", len(data), "bytes:", [hex(b) for b in data])
                    for byte in data:
                        result = self._process_byte(byte)
                        if result is not None and result.get('type') == 'lane_configurations':
                            return result['data']
            time.sleep_ms(5)

        print("uart_comms: timed out waiting for lane_configurations response")
        return None

    def tune_lane(self, lane, band_id, frequency_mhz, timeout_ms=10000):
        """Send a TuneLane command to the comms board.

        Args:
            lane (int):          Lane index (0-based).
            band_id (int):       Band identifier byte.
            frequency_mhz (int): Frequency in MHz (e.g. 5800).
            timeout_ms:          Response timeout in milliseconds (default 10 s).

        Returns:
            True   – comms board acknowledged the tune
            False  – comms board returned an error
            None   – no response within timeout_ms
        """
        payload = struct.pack("<BBH", lane, band_id, frequency_mhz)
        packet = _build_packet(self._crc, TUNE_LANE, payload)
        self._uart.write(packet)
        print("uart_comms: sent tune_lane lane={} band={} freq={}:".format(
            lane, band_id, frequency_mhz), [hex(b) for b in packet])

        deadline = time.ticks_add(time.ticks_ms(), timeout_ms)
        self._rx_buffer = []

        while time.ticks_diff(deadline, time.ticks_ms()) > 0:
            waiting = self._uart.any()
            if waiting > 0:
                data = self._uart.read(waiting)
                if data:
                    print("uart_comms: received", len(data), "bytes:", [hex(b) for b in data])
                    for byte in data:
                        result = self._process_byte(byte)
                        if result is not None:
                            t = result.get('type')
                            if result.get('cmd') == TUNE_LANE:
                                if t == 'ack':
                                    return True
                                elif t == 'error':
                                    return False
            time.sleep_ms(5)

        print("uart_comms: timed out waiting for tune_lane response")
        return None

    # ------------------------------------------------------------------
    # Framing / parsing (mirrors NodeCommunication on the server)
    # ------------------------------------------------------------------

    def _process_byte(self, byte):
        if byte == PACKET_START:
            self._handle_start_of_record()
        elif byte == PACKET_END:
            return self._handle_end_of_record()
        else:
            self._rx_buffer.append(byte)

    def _handle_start_of_record(self):
        if len(self._rx_buffer) > 0:
            if self._rx_buffer[-1] != ESCAPE_BYTE:
                self._rx_buffer = []
        self._rx_buffer.append(PACKET_START)

    def _handle_end_of_record(self):
        if len(self._rx_buffer) > 0:
            self._rx_buffer.append(PACKET_END)
            if self._rx_buffer[-2] != ESCAPE_BYTE:
                packet = self._build_unescaped_packet()
                return self._process_packet(packet)

    def _build_unescaped_packet(self):
        unescaped = bytearray()
        buf = self._rx_buffer
        self._rx_buffer = []
        i = 0
        while i < len(buf):
            byte = buf[i]
            if byte == ESCAPE_BYTE:
                i += 1
                if i < len(buf):
                    unescaped.append(buf[i] - ESCAPE_ADDER)
            elif byte != PACKET_START and byte != PACKET_END:
                unescaped.append(byte)
            i += 1
        return bytes(unescaped)

    def _process_packet(self, packet):
        try:
            header_size = struct.calcsize("<BHH")
            if len(packet) < header_size:
                print("uart_comms: packet too short (", len(packet), "bytes)")
                return None
            command_id, crc, length = struct.unpack("<BHH", packet[:header_size])
            payload = packet[header_size:]
            print("uart_comms: cmd=0x{:02x} crc=0x{:04x} len={} payload={}".format(
                command_id, crc, length, [hex(b) for b in payload]))

            if not _check_payload_crc(self._crc, command_id, crc, length, payload):
                print("uart_comms: CRC mismatch")
                return None
            
            if command_id == RESPONSE:
                if len(payload) < 1:
                    print("uart_comms: empty response payload")
                    return None
                inner_cmd_id = payload[0]
                inner_payload = payload[1:]
                print("uart_comms: response for inner cmd=0x{:02x} payload={}".format(
                    inner_cmd_id, [hex(b) for b in inner_payload]))
                if inner_cmd_id == QUERY_NETWORK_STATUS:
                    expected = struct.calcsize("<BB4s4s4s")
                    if len(inner_payload) < expected:
                        print("uart_comms: network status payload too short", len(inner_payload))
                        return None
                    connected, dhcp, ip_b, subnet_b, gateway_b = struct.unpack("<BB4s4s4s", inner_payload[:expected])
                    ip      = '.'.join(str(b) for b in ip_b)
                    subnet  = '.'.join(str(b) for b in subnet_b)
                    gateway = '.'.join(str(b) for b in gateway_b)
                    status = {
                        'connected': connected != 0,
                        'dhcp':      dhcp != 0,
                        'ip':        ip,
                        'subnet':    subnet,
                        'gateway':   gateway,
                    }
                    print("uart_comms: network status =", status)
                    return {'type': 'network_status', 'data': status}
                elif inner_cmd_id == REQUEST_LANE_CONFIGURATIONS:
                    if len(inner_payload) < 1:
                        print("uart_comms: lane configurations payload too short", len(inner_payload))
                        return None
                    enabled_lanes = inner_payload[0]
                    lane_data = inner_payload[1:]
                    lane_size = struct.calcsize("<BHHH")  # 7 bytes per lane
                    lanes = []
                    offset = 0
                    while offset + lane_size <= len(lane_data):
                        band_id, freq_mhz, entry_thr, exit_thr = struct.unpack(
                            "<BHHH", lane_data[offset:offset + lane_size])
                        lanes.append({
                            'band_id':         band_id,
                            'frequency_mhz':   freq_mhz,
                            'entry_threshold': entry_thr,
                            'exit_threshold':  exit_thr,
                        })
                        offset += lane_size
                    result = {
                        'enabled_lanes': enabled_lanes,
                        'lanes':         lanes,
                    }
                    print("uart_comms: lane configurations =", result)
                    return {'type': 'lane_configurations', 'data': result}
                else:
                    print("uart_comms: unhandled inner cmd 0x{:02x}".format(inner_cmd_id))
            elif command_id == ACK:
                if len(payload) >= 1:
                    inner_cmd_id = payload[0]
                    print("uart_comms: ACK for cmd=0x{:02x}".format(inner_cmd_id))
                    return {'type': 'ack', 'cmd': inner_cmd_id}
            elif command_id == ERROR:
                if len(payload) >= 1:
                    inner_cmd_id = payload[0]
                    print("uart_comms: ERROR for cmd=0x{:02x}".format(inner_cmd_id))
                    return {'type': 'error', 'cmd': inner_cmd_id}
            else:
                print("uart_comms: unexpected command id 0x{:02x}".format(command_id))
        except Exception as e:
            print("uart_comms: parse error", e)
        return None
