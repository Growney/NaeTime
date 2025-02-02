from machine import Pin, SPI
from adafruit import RFM69
import commands
import struct

TUNE_LANE = 0x01
CONFIGURE_NODE = 0x02
LANE_TIMINGS = 0x03
CONFIGURE_LANE_ENTRY_THRESHOLD = 0x04
CONFIGURE_LANE_EXIT_THRESHOLD = 0x05
ERROR = 0xFE
ACK = 0xFF

class RF69NodeCommandHeader:
    def __init__(self, command, crc, length, payload):
        self._command = command
        self._crc = crc
        self._length = length
        self._payload = payload
    

class RFM69NodeCommunication:

    def __init__(self, chip_select_pin, reset_pin, dio0_pin, sck_pin, mosi_pin, miso_pin,frequency,sync_word):
        self._chip_select_pin = Pin(chip_select_pin, Pin.OUT)
        self._reset_pin = Pin(reset_pin, Pin.OUT)
        self._dio0_pin = Pin(dio0_pin, Pin.IN)
        self._spi = SPI(1, baudrate=1_000_000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(sck_pin), mosi=Pin(mosi_pin), miso=Pin(miso_pin))

        self._radio = RFM69(self._spi, self._chip_select_pin, self._reset_pin, self._dio0_pin, frequency)
        self._radio.sync_on = True
        self._radio.sync_word = sync_word
        self._radio.start()

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
            elif command_id == LANE_TIMINGS:
                current_time, lane, rssi, last_pass_start,last_pass_end,pass_state, pass_count = struct.unpack("<BlhllhI", payload)
                return commands.LaneTimings(current_time, lane, rssi, last_pass_start,last_pass_end,pass_state, pass_count)
            elif command_id == CONFIGURE_LANE_ENTRY_THRESHOLD:
                lane, entry_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneEntryThreshold(lane, entry_threshold)
            elif command_id == CONFIGURE_LANE_EXIT_THRESHOLD:
                lane, exit_threshold = struct.unpack("<BH", payload)
                return commands.ConfigureLaneExitThreshold(lane, exit_threshold)
        except Exception as e:
            print("Process Packets Error: ", e)

    def send_ack_for_command(self, command):
        self._send_response(ACK, command)

    def send_error_for_command(self, command):
        self._send_response(ERROR, command)
        
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
        else:
            raise ValueError("Command response Not Supported")
           
        self._send_packet(response, data)

    def send_command(self, command):
        if isinstance(command, commands.LaneTimings):
            command_id = LANE_TIMINGS
            payload = struct.pack("<BlHllBH", command.lane,command.current_time, command.rssi, command.last_pass_start,command.last_pass_end,command.pass_state, command.pass_count)
        else:
            raise ValueError("Unknown command type")
        
        self._send_packet(command_id, payload)

    def _send_packet(self, command_id, payload):  
        packet = struct.pack("<BHH", command_id, 0, len(payload))
        packet += payload
        crc = self._crc.calculate(packet)
        packet = struct.pack("<BHH", command_id, crc, len(payload))
        packet += payload
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

        for i in range(256):
            print(f"0x{table[i]:04X}, ", end="")
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




