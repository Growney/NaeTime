import commands as commands
import struct
from collections import deque 
import uasyncio as asyncio
import usocket as socket

TUNE_LANE = 0x01
CONFIGURE_NODE = 0x02
NODE_TIMINGS = 0x03
CONFIGURE_LANE_ENTRY_THRESHOLD = 0x04
CONFIGURE_LANE_EXIT_THRESHOLD = 0x05
CONFIGURE_LANE_ENABLED = 0x06
INITIALISE_NODE = 0x07
STATUS = 0xFD
ERROR = 0xFE
ACK = 0xFF

class TCPServerSocketRadio:

    def __init__(self, host='0.0.0.0', port=5005):
        self._server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self._server_socket.bind((host, port))
        self._server_socket.listen(1)
        self._server_socket.setblocking(False)
        
        self.rx_received_event = asyncio.Event()
        
        self.rx_queue = deque([],100)
        self.client_socket = None
        print("TCP Server created on {}:{}".format(host, port))

    async def start_tcp_server(self):
        print("Starting TCP server...")
        while True:
            try:
                client_socket, addr = self._server_socket.accept()
                if self.client_socket is None:
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
        self.client_socket = client_socket
        try:
            while True:
                try:
                    data = client_socket.recv(1024)
                    if data:
                        self.rx_queue.append(data)
                        self.rx_received_event.set()
                    else:
                        # Connection closed by client
                        break
                except OSError as e:
                    # No data available (EAGAIN/EWOULDBLOCK)
                    pass
                await asyncio.sleep_ms(100)
        finally:
            print("Client disconnected:", client_addr)
            self.client_socket = None
            try:
                client_socket.close()
            except:
                pass
    
    async def wait_for_rx(self):
        if(len(self.rx_queue) == 0):
            await self.rx_received_event.wait()
            self.rx_received_event.clear()

        return self.rx_queue.popleft()
        
    def send(self, data):
        if self.client_socket is not None:
            try:
                self.client_socket.send(data)
            except OSError as e:
                print("Failed to send to client:", e)
                try:
                    self.client_socket.close()
                except:
                    pass
                self.client_socket = None

class NodeCommunication:

    def __init__(self, radio):
        self._radio = radio
        self._crc = CRC16()
    
    async def wait_for_command(self):
        try:
            packet = await self._radio.wait_for_rx()
            command_id, crc, length, payload = struct.unpack("<BHH" + str(len(packet) - struct.calcsize("<BHH")) + "s", packet)
            if command_id == TUNE_LANE:
                lane, frequency = struct.unpack("<BH", payload)
                return commands.TuneLane(lane, frequency)
            elif command_id == CONFIGURE_NODE:
                node_id, transmit_frequency, polling_frequency = struct.unpack("<cii", payload)
                return commands.ConfigureNode(node_id, transmit_frequency, polling_frequency)
            elif command_id == CONFIGURE_LANE_ENTRY_THRESHOLD:
                lane, entry_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneEntryThreshold(lane, entry_threshold)
            elif command_id == CONFIGURE_LANE_EXIT_THRESHOLD:
                lane, exit_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneExitThreshold(lane, exit_threshold)
            elif command_id == CONFIGURE_LANE_ENABLED:
                lane, enabled = struct.unpack("<BB", payload)
                return commands.ConfigureLaneEnabled(lane, enabled)
            elif command_id == INITIALISE_NODE:
                node_template = "<BB"
                node_info_size = struct.calcsize(node_template)
                enabled_lanes, lane_count = struct.unpack(node_template, payload)
                lane_infos = []
                for i in range(lane_count):
                    lane_template = "<HHH"
                    lane_info_size = struct.calcsize(lane_template)
                    frequency_in_mhz, entry_threshold, exit_threshold = struct.unpack(lane_template, payload[node_info_size + i*lane_info_size:])
                    lane_infos.append(commands.LaneConfiguration(frequency_in_mhz, entry_threshold, exit_threshold))
                return commands.InitialiseNode(enabled_lanes, lane_count, lane_infos)
        except Exception as e:
            print("Process Packets Error: ", e)

    def send_ack_for_command(self, command):
        self._send_response(ACK, command)

    def send_error_for_command(self, command):
        self._send_response(ERROR, command)

    def send_status_for_command(self, command):
        self._send_response(STATUS, command)
        
    def _send_response(self, response, command):
        if(isinstance(command, commands.TuneLane)):
            print("Sending tune response:", response)
            data = struct.pack("<BBH",TUNE_LANE, command.lane, command.frequency_in_mhz)
        elif(isinstance(command, commands.ConfigureLaneEntryThreshold)):
            print("Sending entry threshold response:", response)
            data = struct.pack("<BBH",CONFIGURE_LANE_ENTRY_THRESHOLD, command.lane, command.entry_threshold)
        elif(isinstance(command, commands.ConfigureLaneExitThreshold)):
            print("Sending exit threshold response:", response)
            data = struct.pack("<BBH",CONFIGURE_LANE_EXIT_THRESHOLD, command.lane, command.exit_threshold)
        elif(isinstance(command, commands.ConfigureLaneEnabled)):
            print("Sending lane enabled response:", response)
            data = struct.pack("<BBB",CONFIGURE_LANE_ENABLED, command.lane, command.enabled)
        elif(isinstance(command, commands.InitialiseNode)):
            print("Send initialise node response:", response)
            data = struct.pack("<B",INITIALISE_NODE)
        else:
            raise ValueError("Command response Not Supported")
           
        self._send_packet(response, data)

    def send_command(self, command):
        if isinstance(command, commands.NodeTimings):
            command_id = NODE_TIMINGS

            payload = struct.pack("<LBB", command.current_time, command.lane_count, command.enabled_lanes)

            for lane_timing in command.lane_timings:
                payload += struct.pack("<HLH", lane_timing.rssi, lane_timing.last_pass, lane_timing.pass_count)

        else:
            raise ValueError("Unknown command type")
        
        self._send_packet(command_id, payload)

    def _send_packet(self, command_id, payload):  
        packet = struct.pack("<BHH", command_id, 0, len(payload))
        packet += payload
        crc = self._crc.calculate(packet)
        packet = struct.pack("<BBHH", 90, command_id, crc, len(payload))
        packet += payload
        packet += struct.pack("<B", 91)
        self._radio.send(packet)

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




