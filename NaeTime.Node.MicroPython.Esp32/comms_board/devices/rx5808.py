from machine import Pin
import time

T1_DELAY_NANOSECONDS = 20
T2_DELAY_NANOSECONDS = 20
T3_DELAY_NANOSECONDS = 30
T4_DELAY_NANOSECONDS = 30
#Datasheet specifies 100ns but testing shows 10 microseconds is not long enough
T5_DELAY_NANOSECONDS = 100
#Datasheet specifies 20ns but testing shows 10 microseconds is not long enough
T6_DELAY_NANOSECONDS = 20
T7_DELAY_NANOSECONDS = 100

#Datasheet delays are in nanoseconds, in an attempt to be more consise with timing the best we can do is divide the nanoseconds by a factor and delay in microseconds
DELAY_FACTOR = 10  

MINIMUM_BETWEEN_DATA_AND_CLOCK_HIGH = T1_DELAY_NANOSECONDS // DELAY_FACTOR
MINIMUM_TIME_BETWEEN_CLOCK_PULSE_HIGH_AND_DATA = T2_DELAY_NANOSECONDS // DELAY_FACTOR
MINIMUM_CLOCK_HIGH_TIME = T3_DELAY_NANOSECONDS // DELAY_FACTOR
MINIMUM_BETWEEN_DATA_LAST_CLOCK_HIGH_AND_CHIP_SELECT_HIGH = T4_DELAY_NANOSECONDS // DELAY_FACTOR
#Dataset specifies T4 should be used here but it seems that it takes a minimum of 30ms between transactions
MINIMUM_CHIP_SELECT_PULSE_TIME = 30000
MINIMUM_BETWEEN_CHIP_SELECT_LOW_AND_CLOCK_HIGH = T6_DELAY_NANOSECONDS // DELAY_FACTOR
MINIMUM_BETWEEN_CLOCK_PULSES = T7_DELAY_NANOSECONDS // DELAY_FACTOR

MINIMUM_TIME_TO_CHANGE_PIN_MODE = 0  # microseconds

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
        self._clock_pin = Pin(clock_pin, Pin.OUT, value=0)
        self._data_pin = Pin(data_pin, Pin.OUT, value=0)
        self._is_data_in_write = True
        self._select_pin = Pin(select_pin, Pin.OUT, value=1)
        current = time.ticks_us()
        self._data_set_time = current;
        self._clock_high_time = current;
        self._chip_select_high_time = current;
        self._chip_select_low_time = current;
        self._data_pin_mode_changed_time = current;
    
        
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
        current = time.ticks_us()
        time_since_chip_select_low = current - self._chip_select_low_time
        time_since_data_set = current - self._data_set_time
        time_since_last_clock_high = current - self._clock_high_time

        if(time_since_chip_select_low < MINIMUM_BETWEEN_CHIP_SELECT_LOW_AND_CLOCK_HIGH or time_since_data_set < MINIMUM_BETWEEN_DATA_AND_CLOCK_HIGH or time_since_last_clock_high < MINIMUM_BETWEEN_CLOCK_PULSES):
            
            data_delay = MINIMUM_BETWEEN_DATA_AND_CLOCK_HIGH - time_since_data_set
            chip_select_low_delay = MINIMUM_BETWEEN_CHIP_SELECT_LOW_AND_CLOCK_HIGH - time_since_chip_select_low
            clock_high_delay = MINIMUM_BETWEEN_CLOCK_PULSES - time_since_last_clock_high

            delay = max(data_delay,chip_select_low_delay, clock_high_delay)
            time.sleep_us(delay)

        self._clock_pin.value(1)
        self._clock_high_time = time.ticks_us() 
        time.sleep_us(MINIMUM_CLOCK_HIGH_TIME)
        self._clock_pin.value(0)

    def _set_select(self,is_high):
        current = time.ticks_us()
        if(is_high):
            time_since_last_clock_high = current - self._clock_high_time
            if(time_since_last_clock_high < MINIMUM_BETWEEN_DATA_LAST_CLOCK_HIGH_AND_CHIP_SELECT_HIGH):
                delay = MINIMUM_BETWEEN_DATA_LAST_CLOCK_HIGH_AND_CHIP_SELECT_HIGH - time_since_last_clock_high
                time.sleep_us(delay)
            self._select_pin.value(1)
            self._chip_select_high_time = time.ticks_us()
        else:
            time_since_chip_select_high = current - self._chip_select_high_time
            if(time_since_chip_select_high < MINIMUM_CHIP_SELECT_PULSE_TIME):
                delay = MINIMUM_CHIP_SELECT_PULSE_TIME - time_since_chip_select_high
                time.sleep_us(delay)
            self._select_pin.value(0)
            self._chip_select_low_time = time.ticks_us()
    
    def _check_for_data_pin_usage_delay(self):
        current = time.ticks_us()
        time_since_clock_high = current - self._clock_high_time
        time_since_mode_change = current - self._data_pin_mode_changed_time
        if(time_since_clock_high < MINIMUM_TIME_BETWEEN_CLOCK_PULSE_HIGH_AND_DATA or time_since_mode_change < MINIMUM_TIME_TO_CHANGE_PIN_MODE):
            time_since_clock_delay = MINIMUM_TIME_BETWEEN_CLOCK_PULSE_HIGH_AND_DATA - time_since_clock_high
            mode_change_delay = MINIMUM_TIME_TO_CHANGE_PIN_MODE - time_since_mode_change
            delay = max(time_since_clock_delay, mode_change_delay)
            time.sleep_us(delay)
    def _set_data_pin_low(self):
        self._check_for_data_pin_usage_delay()
        self._data_pin.value(0)
        self._data_set_time = time.ticks_us()

    def _write_bit(self, value):
        self._check_for_data_pin_usage_delay()
        if(value):
            self._data_pin.value(1)
        else:
            self._data_pin.value(0)
        self._data_set_time = time.ticks_us()
        self._pulse_clock_pin()

    def _read_bit(self):
        self._check_for_data_pin_usage_delay()
        pin_value = self._data_pin.value()
        self._pulse_clock_pin()
        self._data_set_time = time.ticks_us()
        return pin_value == 1
    
    def _setup_data_pin_for_read(self):
        # Always reinitialize because other modules may have changed the shared pin state
        self._is_data_in_write = False
        self._data_pin.init(Pin.IN, Pin.PULL_UP)
        self._data_pin_mode_changed_time = time.ticks_us()

    def _setup_data_pin_for_write(self):
        # Always reinitialize because other modules may have changed the shared pin state
        self._is_data_in_write = True
        self._data_pin.init(Pin.OUT, value=0)
        self._data_pin_mode_changed_time = time.ticks_us()

    def _send_register_address(self, address):
        for x in range(0,4):
            write_bit = (address >> x) & 0x01 == 0x01
            self._write_bit(write_bit)

    def _read_from_register(self, address):
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
        return result

    def _write_to_register(self, address, value):
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
    
    @staticmethod
    def confirm_matched_register_frequencies(target_frequency, registered_frequency):
        return Rx5808RegisterCommunication.calculate_frequency_register_value(target_frequency) == Rx5808RegisterCommunication.calculate_frequency_register_value(registered_frequency)
    
        
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