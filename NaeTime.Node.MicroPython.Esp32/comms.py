from machine import Pin, SPI
from adafruit import RFM69
import commands
import struct

TUNE_LANE = 0x01
CONFIGURE_NODE = 0x02
LANE_TIMINGS = 0x03

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
    
    async def wait_for_command(self):
        try:
            packet = await self._radio.wait_for_rx()
            
            command_id, crc, length, payload = struct.unpack("<BHH" + str(len(packet) - struct.calcsize("<BHH")) + "s", packet)
            if command_id == TUNE_LANE:
                lane, frequency = struct.unpack("<ci", payload)
                return commands.TuneLane(lane, frequency)
            elif command_id == CONFIGURE_NODE:
                node_id, transmit_frequency, polling_frequency = struct.unpack("<cii", payload)
                return commands.ConfigureNode(node_id, transmit_frequency, polling_frequency)
            elif command_id == LANE_TIMINGS:
                current_time, lane, rssi, last_pass_start,last_pass_end,pass_state, pass_count = struct.unpack("<BlhllhI", payload)
                return commands.LaneTimings(current_time, lane, rssi, last_pass_start,last_pass_end,pass_state, pass_count)
            
            print("Received: ", packet)
        except Exception as e:
            print("Process Packets Error: ", e)

    def send_command(self, command):
        if isinstance(command, commands.TuneLane):
            command_id= TUNE_LANE
            payload = struct.pack("<ci", command.lane, command.frequency_in_mhz)
        elif isinstance(command, commands.ConfigureNode):
            command_id = CONFIGURE_NODE
            payload = struct.pack("<cii", command.node_id, command.transmit_frequency_hz,command.polling_frequency_hz)
        elif isinstance(command, commands.LaneTimings):
            command_id = LANE_TIMINGS
            payload = struct.pack("<BlhllhI", command.lane,command.current_time, command.rssi, command.last_pass_start,command.last_pass_end,command.pass_state, command.pass_count)
        else:
            raise ValueError("Unknown command type")
        
        print("Payload bytes: ", len(payload))
        packet = struct.pack("<BHH", command_id, 0, len(payload))
        packet += payload

        print("Sending bytes: ", len(packet))

        self._radio.send(packet)

        




