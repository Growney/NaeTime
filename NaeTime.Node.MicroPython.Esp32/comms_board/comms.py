import commands as commands
import struct
from collections import deque 
import uasyncio as asyncio
import usocket as socket

TUNE_LANE = 0x01
CONFIGURE_NODE = 0x02
NODE_STATUS = 0x03
CONFIGURE_LANE_ENTRY_THRESHOLD = 0x04
CONFIGURE_LANE_EXIT_THRESHOLD = 0x05
CONFIGURE_LANE_ENABLED = 0x06
REQUEST_LANE_CONFIGURATIONS = 0x07
LANE_PASS_EVENT = 0x08

RESPONSE = 0xFC
STATUS = 0xFD
ERROR = 0xFE
ACK = 0xFF

# UI-board <-> comms-board UART protocol
QUERY_NETWORK_STATUS = 0x20
NETWORK_STATUS_RESPONSE = 0x21
CONFIGURE_NETWORK = 0x22

class TCPServerSocketRadio:

    def __init__(self, host='0.0.0.0', port=5005):
        self._server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self._server_socket.bind((host, port))
        self._server_socket.listen(1)
        self._server_socket.setblocking(False)
        
        self._rx_received_event = asyncio.Event()
        
        self._rx_queue = deque([],100)
        self._client_socket = None
        print("TCP Server created on {}:{}".format(host, port))

    async def start_tcp_server(self):
        print("Starting TCP server...")
        while True:
            try:
                client_socket, addr = self._server_socket.accept()
                if self._client_socket is None:
                    print("Client connected from:", addr)
                    asyncio.create_task(self._handle_client(client_socket, addr))
                else:
                    print("Rejecting connection from {} - client already connected".format(addr))
                    client_socket.close()
            except OSError:
                await asyncio.sleep_ms(100)
    
    async def _handle_client(self, client_socket, client_addr):
        print("Client handler started for:", client_addr)
        client_socket.setblocking(False)
        self._client_socket = client_socket
        try:
            while True:
                try:
                    data = client_socket.recv(1024)
                    if data:
                        self._rx_queue.append(data)
                        self._rx_received_event.set()
                    else:
                        # Connection closed by client
                        break
                except OSError as e:
                    # No data available (EAGAIN/EWOULDBLOCK)
                    pass
                await asyncio.sleep_ms(100)
        finally:
            print("Client disconnected:", client_addr)
            self._client_socket = None
            try:
                client_socket.close()
            except:
                pass
    
    async def wait_for_rx(self):
        if(len(self._rx_queue) == 0):
            await self._rx_received_event.wait()
            self._rx_received_event.clear()

        return self._rx_queue.popleft()
        
    def send(self, data):
        if self._client_socket is not None:
            try:
                self._client_socket.send(data)
            except OSError as e:
                print("Failed to send to client:", e)
                try:
                    self._client_socket.close()
                except:
                    pass
                self._client_socket = None

class UARTRadio:
    """Async UART transport.  Reads bytes from the hardware UART and queues
    them so that NodeCommunication can consume them with wait_for_rx().
    Start the background reader task by calling start() inside an asyncio loop.
    """

    def __init__(self, uart):
        self._uart = uart
        self._rx_received_event = asyncio.Event()
        self._rx_queue = deque([], 100)

    async def start(self):
        """Background coroutine – run as an asyncio task."""
        while True:
            waiting = self._uart.any()
            if waiting > 0:
                # Read exactly the bytes already in the buffer.
                # read() with no argument blocks until the UART's timeout
                # (often 1 s) which would stall the entire event loop.
                data = self._uart.read(waiting)
                if data:
                    self._rx_queue.append(data)
                    self._rx_received_event.set()
            await asyncio.sleep_ms(5)

    async def wait_for_rx(self):
        if len(self._rx_queue) == 0:
            await self._rx_received_event.wait()
            self._rx_received_event.clear()
        rx = self._rx_queue.popleft()
        print("UARTRadio received data:", [hex(b) for b in rx])
        return rx

    def send(self, data):
        print("UARTRadio sending data:", [hex(b) for b in data])
        self._uart.write(data)
        # Ensure all bytes are physically transmitted before yielding
        # back to the asyncio event loop.
        try:
            self._uart.flush()
        except AttributeError:
            pass  # flush() not available on this port


class NodeCommunication:

    PACKET_START = 0x5A
    PACKET_END = 0x5B
    ESCAPE_BYTE = 0x5C
    ESCAPE_ADDER = 0x40

    def __init__(self, radio):
        self._radio = radio
        self._crc = CRC16()
        self._rx_buffer = []
    
    async def wait_for_command(self):
        try:
            rxData = await self._radio.wait_for_rx()
            for byte in rxData:
                command = self._process_byte(byte)
                if command is not None:
                    return command
            
        except Exception as e:
            print("Process Packets Error: ", e)
    
    def _process_byte(self, byte):
        if(byte == self.PACKET_START):
            return self._handle_start_of_record()
        elif(byte == self.PACKET_END):
            return self._handle_end_of_record()
        else:
            self._rx_buffer.append(byte)
    
    def _handle_start_of_record(self):
        if(len(self._rx_buffer) > 0):
            previous_byte = self._rx_buffer[-1]
            if(previous_byte != self.ESCAPE_BYTE):
                self._rx_buffer = []
        
        self._rx_buffer.append(self.PACKET_START)
    
    def _handle_end_of_record(self):
        if(len(self._rx_buffer) > 0):
            previous_byte = self._rx_buffer[-1]
            self._rx_buffer.append(self.PACKET_END)

            if(previous_byte != self.ESCAPE_BYTE):
                packet = self._build_unescaped_packet_from_buffer()
                return self._process_packet(packet)
        

    def _build_unescaped_packet_from_buffer(self):
        unescaped_packet = bytearray()
        
        while(len(self._rx_buffer) > 0):
            byte = self._rx_buffer.pop(0)
            if byte == self.ESCAPE_BYTE:
                if len(self._rx_buffer) > 0:
                    next_byte = self._rx_buffer.pop(0)
                    unescaped_packet.append(next_byte - self.ESCAPE_ADDER)
            elif byte != self.PACKET_START and byte != self.PACKET_END:
                unescaped_packet.append(byte)
        
        return unescaped_packet

    def _check_payload_crc(self,command_id, received_crc,length, payload):
        try:
            crc_data = struct.pack("<BHH", command_id, 0, length) + payload
            calculated_crc = self._crc.calculate(crc_data)
            return received_crc == calculated_crc
        except Exception as e:
            print("CRC Check Error: ", e)
            return False

    def _process_packet(self, packet):
        try:
            print("Processing Packet:", packet)
            command_id, crc, length, payload = struct.unpack("<BHH" + str(len(packet) - struct.calcsize("<BHH")) + "s", packet)

            if not self._check_payload_crc(command_id, crc, length, payload):
                print("CRC Mismatch")
                return None
            print("Packet CRC Valid")
            if command_id == TUNE_LANE:
                print("Building TUNE_LANE Command")
                lane, bandId, frequency = struct.unpack("<BBH", payload)
                return commands.TuneLane(lane, bandId, frequency)
            elif command_id == CONFIGURE_NODE:
                print("Building CONFIGURE_NODE Command")
                node_id, transmit_frequency, polling_frequency = struct.unpack("<cii", payload)
                return commands.ConfigureNode(node_id, transmit_frequency, polling_frequency)
            elif command_id == CONFIGURE_LANE_ENTRY_THRESHOLD:
                print("Building CONFIGURE_LANE_ENTRY_THRESHOLD Command")
                lane, entry_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneEntryThreshold(lane, entry_threshold)
            elif command_id == CONFIGURE_LANE_EXIT_THRESHOLD:
                print("Building CONFIGURE_LANE_EXIT_THRESHOLD Command")
                lane, exit_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneExitThreshold(lane, exit_threshold)
            elif command_id == CONFIGURE_LANE_ENABLED:
                print("Building CONFIGURE_LANE_ENABLED Command")
                lane, enabled = struct.unpack("<BB", payload)
                return commands.ConfigureLaneEnabled(lane, enabled)
            elif command_id == REQUEST_LANE_CONFIGURATIONS:
                print("Building REQUEST_LANE_CONFIGURATIONS Command")
                lanes = struct.unpack("<B",payload)
                return commands.RequestLaneConfigurations(lanes)
            elif command_id == QUERY_NETWORK_STATUS:
                print("Building QUERY_NETWORK_STATUS Command")
                return commands.QueryNetworkStatus()
            elif command_id == CONFIGURE_NETWORK:
                print("Building CONFIGURE_NETWORK Command")
                dhcp, ip_b, subnet_b, gateway_b = struct.unpack("<B4s4s4s", payload)
                ip = '.'.join(str(b) for b in ip_b)
                subnet = '.'.join(str(b) for b in subnet_b)
                gateway = '.'.join(str(b) for b in gateway_b)
                return commands.ConfigureNetwork(dhcp != 0, ip, subnet, gateway)
        except Exception as e:
            print("Process Packets Error: ", e)

    def send_ack_for_command(self, command):
        self._send_response(ACK, command)

    def send_error_for_command(self, command):
        self._send_response(ERROR, command)

    def send_status_for_command(self, command):
        self._send_response(STATUS, command)
    def send_response_for_command(self, command):
        self._send_response(RESPONSE, command)
        
    def _send_response(self, response, command):
        print("Preparing to send response:", response, "for command:", command)
        if(isinstance(command, commands.TuneLane)):
            print("Sending tune response:", response)
            data = struct.pack("<BBBH",TUNE_LANE, command.lane, command.bandId, command.frequency_in_mhz)
        elif(isinstance(command, commands.ConfigureLaneEntryThreshold)):
            print("Sending entry threshold response:", response)
            data = struct.pack("<BBH",CONFIGURE_LANE_ENTRY_THRESHOLD, command.lane, command.entry_threshold)
        elif(isinstance(command, commands.ConfigureLaneExitThreshold)):
            print("Sending exit threshold response:", response)
            data = struct.pack("<BBH",CONFIGURE_LANE_EXIT_THRESHOLD, command.lane, command.exit_threshold)
        elif(isinstance(command, commands.ConfigureLaneEnabled)):
            print("Sending lane enabled response:", response)
            data = struct.pack("<BBB",CONFIGURE_LANE_ENABLED, command.lane, command.enabled)
        elif(isinstance(command, commands.LaneConfigurationsResponse)):
            print("Send lane configuration response:", response)
            data = struct.pack("<BB", REQUEST_LANE_CONFIGURATIONS, command.enabled_lanes)
            for lane_config in command.lane_configurations:
                data += struct.pack("<BHHH", lane_config.bandId,lane_config.frequency_in_mhz, lane_config.entry_threshold, lane_config.exit_threshold)
        elif(isinstance(command, commands.NetworkStatusResponse)):
            print("Sending network status response:", response)
            ip_bytes = bytes(int(x) for x in command.ip.split('.'))
            subnet_bytes = bytes(int(x) for x in command.subnet.split('.'))
            gateway_bytes = bytes(int(x) for x in command.gateway.split('.'))
            data = struct.pack("<BBB4s4s4s", QUERY_NETWORK_STATUS,
                               1 if command.connected else 0,
                               1 if command.dhcp else 0,
                               ip_bytes, subnet_bytes, gateway_bytes)
        elif(isinstance(command, commands.ConfigureNetwork)):
            print("Sending configure network response:", response)
            ip_bytes = bytes(int(x) for x in command.ip.split('.'))
            subnet_bytes = bytes(int(x) for x in command.subnet.split('.'))
            gateway_bytes = bytes(int(x) for x in command.gateway.split('.'))
            data = struct.pack("<BB4s4s4s", CONFIGURE_NETWORK,
                               1 if command.dhcp else 0,
                               ip_bytes, subnet_bytes, gateway_bytes)
        else:
            raise ValueError("Command response Not Supported")
           
        self._send_packet(response, data)

    def send_command(self, command):
        if isinstance(command, commands.NodeStatus):
            command_id = NODE_STATUS
            payload = struct.pack("<QBB", command.current_time, command.lane_count, command.enabled_lanes)
            for lane_timing in command.lane_statuses:
                payload += struct.pack("<BQH",lane_timing.lane, lane_timing.rssi_read_time, lane_timing.rssi)

        elif isinstance(command, commands.LanePassEvent):
            command_id = LANE_PASS_EVENT
            payload = struct.pack("<BHQQ", command.lane, command.pass_count, command.start_time, command.end_time)

        elif isinstance(command, commands.NetworkStatusResponse):
            command_id = NETWORK_STATUS_RESPONSE
            ip_bytes = bytes(int(x) for x in command.ip.split('.'))
            subnet_bytes = bytes(int(x) for x in command.subnet.split('.'))
            gateway_bytes = bytes(int(x) for x in command.gateway.split('.'))
            payload = struct.pack("<BB4s4s4s",
                                  1 if command.connected else 0,
                                  1 if command.dhcp else 0,
                                  ip_bytes, subnet_bytes, gateway_bytes)

        else:
            raise ValueError("Unknown command type")
        
        self._send_packet(command_id, payload)

    def _create_packet_payload(self, command_id, payload):
        packet = struct.pack("<BHH", command_id, 0, len(payload))
        packet += payload
        crc = self._crc.calculate(packet)
        packet = struct.pack("<BHH", command_id, crc, len(payload))
        packet += payload
        return packet
    
    def _escape_packet(self, packet):
        payload = struct.pack("<B", self.PACKET_START)
        for byte in packet:
            if byte == self.PACKET_START or byte == self.PACKET_END or byte == self.ESCAPE_BYTE or byte == 0x03:
                payload += struct.pack("<BB", self.ESCAPE_BYTE, byte + self.ESCAPE_ADDER)
            else:
                payload += struct.pack("<B", byte)

        payload += struct.pack("<B", self.PACKET_END)
        return payload
    
    def _send_packet(self, command_id, payload):

        payload = self._create_packet_payload(command_id, payload)
        payload = self._escape_packet(payload)
        self._radio.send(payload)


class CRC16:
    def __init__(self, polynomial=0x8005, initial_value=0xFFFF):
        """
        Initialize the CRC-16 calculator.

        :param polynomial: The polynomial to use for the CRC calculation.
        :param initial_value: The initial value for the CRC calculation.
        """
        self.polynomial = polynomial
        self.initial_value = initial_value
        self.table = self._create_table()

    def _create_table(self):
        """
        Create the CRC-16 lookup table.

        :return: The CRC-16 lookup table.
        """
        table = []
        for byte in range(256):
            crc = 0
            for bit in range(8):
                if (byte ^ crc) & 0x01:
                    crc = (crc >> 1) ^ self.polynomial
                else:
                    crc >>= 1
                byte >>= 1
            table.append(crc)

        return table

    def reflect(self, data, width):
        """
        Reflect the lower 'width' bits of 'data'.

        :param data: The data to reflect.
        :param width: The number of bits to reflect.
        :return: The reflected data.
        """
        reflection = 0
        for i in range(width):
            if data & (1 << i):
                reflection |= 1 << (width - 1 - i)
        return reflection

    def calculate(self, data):
        """
        Calculate the CRC-16 checksum for the given data.

        :param data: The data to calculate the CRC for.
        :return: The CRC-16 checksum.
        """
        crc = self.initial_value
        for byte in data:
            byte = self.reflect(byte, 8)
            crc = (crc >> 8) ^ self.table[(crc ^ byte) & 0xFF]
        crc = self.reflect(crc, 16)
        return crc




