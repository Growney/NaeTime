from machine import Pin
import time

COMUNICATION_DELAY_MICROSECONDS = 10
TRANSACTION_DELAY_MILLISECONDS = 25

SYNTHESIZERA = 0x00
SYNTHESIZERB = 0x01
SYNTHESIZERC = 0x02
SYNTHESIZERD = 0x03
VCO_SWITCH_CAP_CONTROL = 0x04
DFC_CONTROL = 0x05
SIXM_AUDIO_DEMODULATOR_CONTROL = 0x06
SIXM_FIVE_AUDIO_DEMODULATOR_CONTROL = 0x07
RECEIVER_CONTROL_ONE = 0x08
RECEIVER_CONTROL_TWO = 0x09
POWER_DOWN_CONTROL = 0x0A
STATE = 0x0F

class Rx5808RegisterCommunication:

    def __init__(self, clock_pin, data_pin, select_pin) :
        self._clock_pin = Pin(clock_pin, Pin.OUT)
        self._data_pin = Pin(data_pin, Pin.OUT)
        self._is_data_in_write = True
        self._last_transaction = 0
        self._select_pin = Pin(select_pin, Pin.OUT)
        
    def init(self):
        self._write_to_register(POWER_DOWN_CONTROL,0b11010000110111110011)
    
    def set_frequency(self,frequency_in_MHz):
        register_value = Rx5808RegisterCommunication.calculate_frequency_register_value(frequency_in_MHz)
        self._write_to_register(SYNTHESIZERB, register_value)
    
    def confirm_frequency(self,frequency_in_MHz):
        register_value = self._read_from_register(SYNTHESIZERB)
        return register_value == Rx5808RegisterCommunication.calculate_frequency_register_value(frequency_in_MHz)
    
    def get_stored_frequency(self):
        register_value = self._read_from_register(SYNTHESIZERB)
        return Rx5808RegisterCommunication.calculate_register_value_frequency(register_value)
    
    def tune(self,frequency_in_MHz):
        self.set_frequency(frequency_in_MHz)
        return self.confirm_frequency(frequency_in_MHz)

    def _pulse_clock_pin(self):
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        self._clock_pin.value(1)
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        self._clock_pin.value(0)

    def _set_select(self,is_high):
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        if(is_high):
            self._select_pin.value(1)
        else:
            self._select_pin.value(0)
    
    def _write_bit(self, value):
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        if(value):
            self._data_pin.value(1)
        else:
            self._data_pin.value(0)
        self._pulse_clock_pin()

    def _read_bit(self):
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        pin_value = self._data_pin.value()
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        self._pulse_clock_pin()
        return pin_value == 1
    
    def _setup_data_pin_for_read(self):
        if not (self._is_data_in_write):
            return
        
        self._is_data_in_write = False
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        self._data_pin.init(Pin.IN, Pin.PULL_UP)

    def _setup_data_pin_for_write(self):
        if(self._is_data_in_write):
            return
        
        self._is_data_in_write = True
        time.sleep_us(COMUNICATION_DELAY_MICROSECONDS)
        self._data_pin.init(Pin.OUT)
    
    def _send_register_address(self, address):
        for x in range(0,4):
            write_bit = (address >> x) & 0x01 == 0x01
            self._write_bit(write_bit)
    
    def _check_transaction_delay(self):
        current = time.ticks_ms()
        time_since = current - self._last_transaction
        if(time_since < TRANSACTION_DELAY_MILLISECONDS):
            time.sleep_ms(TRANSACTION_DELAY_MILLISECONDS - time_since)

    def _read_from_register(self, address):
        self._check_transaction_delay()
        self._set_select(True)

        self._setup_data_pin_for_write()
        self._set_select(False)

        self._send_register_address(address)

        self._write_bit(False)

        self._setup_data_pin_for_read()

        result = 0
        for location in range(0,20):
            read_value = self._read_bit()
            if(read_value):
                result |= 1 << location
        
        self._set_select(True)
        self._last_transaction = time.ticks_ms()
        return result

    def _write_to_register(self, address, value):
        self._check_transaction_delay()
        self._set_select(True)

        self._setup_data_pin_for_write()
        self._set_select(False)

        self._send_register_address(address)

        self._write_bit(True)

        for location in range(0,20):
            if((value & (1 << location)) == (1 << location)):
                self._write_bit(True)
            else:
                self._write_bit(False)
        
        self._set_select(True)
        self._last_transaction = time.ticks_ms()

    @staticmethod
    def calculate_frequency_register_value(frequency_in_MHz):
        tf = (frequency_in_MHz - 479) // 2
        n = tf // 32
        a = tf % 32
        return (n << 7) + a
    
    @staticmethod
    def calculate_register_value_frequency(register_value):
        n = register_value >> 7
        a = register_value & 0x7f

        tf = (n*32) + a

        return (tf * 2) + 479
    
        
class Rx5808RegisterCommunicationGroup:

    def __init__(self,clock_pin:int,data_pin:int,*select_pins:int):
        self._registers = []  # type: list[Rx5808RegisterCommunication]
        for select_pin in select_pins:
            new_register = Rx5808RegisterCommunication(clock_pin,data_pin,select_pin)
            self._registers.append(new_register)
    
    def tune(self,device_index,frequency_in_MHz):
        return self._registers[device_index].tune(frequency_in_MHz)
    
    def get_frequency(self,device_index):
        return self._registers[device_index].get_stored_frequency()