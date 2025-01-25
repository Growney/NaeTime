
# The MIT License (MIT)
#
# Copyright (c) 2017 Tony DiCola for Adafruit Industries
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

import time
import random
from collections import deque # type: ignore
from micropython import const
from machine import Pin
import asyncio

# Internal constants:
_REG_FIFO = const(0x00)
_REG_OP_MODE = const(0x01)
_REG_DATA_MOD = const(0x02)
_REG_BITRATE_MSB = const(0x03)
_REG_BITRATE_LSB = const(0x04)
_REG_FDEV_MSB = const(0x05)
_REG_FDEV_LSB = const(0x06)
_REG_FRF_MSB = const(0x07)
_REG_FRF_MID = const(0x08)
_REG_FRF_LSB = const(0x09)
_REG_VERSION = const(0x10)
_REG_PA_LEVEL = const(0x11)
_REG_RX_BW = const(0x19)
_REG_AFC_BW = const(0x1A)
_REG_RSSI_VALUE = const(0x24)
_REG_DIO_MAPPING1 = const(0x25)
_REG_IRQ_FLAGS1 = const(0x27)
_REG_IRQ_FLAGS2 = const(0x28)
_REG_PREAMBLE_MSB = const(0x2C)
_REG_PREAMBLE_LSB = const(0x2D)
_REG_SYNC_CONFIG = const(0x2E)
_REG_SYNC_VALUE1 = const(0x2F)
_REG_PACKET_CONFIG1 = const(0x37)
_REG_FIFO_THRESH = const(0x3C)
_REG_PACKET_CONFIG2 = const(0x3D)
_REG_AES_KEY1 = const(0x3E)
_REG_TEMP1 = const(0x4E)
_REG_TEMP2 = const(0x4F)
_REG_TEST_PA1 = const(0x5A)
_REG_TEST_PA2 = const(0x5C)
_REG_TEST_DAGC = const(0x6F)

_TEST_PA1_NORMAL = const(0x55)
_TEST_PA1_BOOST = const(0x5D)
_TEST_PA2_NORMAL = const(0x70)
_TEST_PA2_BOOST = const(0x7C)

# The crystal oscillator frequency and frequency synthesizer step size.
# See the datasheet for details of this calculation.
_FXOSC = 32000000.0
_FSTEP = _FXOSC / 524288

_DIO_MAPPING_DIO0_RX_CRCOK = 0b00
_DIO_MAPPING_DIO0_RX_PAYLOADREADY =0b01
_DIO_MAPPING_DIO0_RX_SYNCADDRESS = 0b10
_DIO_MAPPING_DIO0_RX_RSSI = 0b11

_DIO_MAPPING_DIO0_TX_PACKETSENT = 0b00
_DIO_MAPPING_DIO0_TX_TXREADY = 0b01
_DIO_MAPPING_DIO0_TX_PLLLOCK = 0b11
# User facing constants:
SLEEP_MODE = 0b000
STANDBY_MODE = 0b001
FS_MODE = 0b010
TX_MODE = 0b011
RX_MODE = 0b100


class RFM69:


    class _RegisterBits:
        def __init__(self, address, *, offset=0, bits=1):
            assert 0 <= offset <= 7
            assert 1 <= bits <= 8
            assert (offset + bits) <= 8
            self._address = address
            self._mask = 0
            for _ in range(bits):
                self._mask <<= 1
                self._mask |= 1
            self._mask <<= offset
            self._offset = offset

        def __get__(self, obj, objtype):
            reg_value = obj._read_u8(self._address)
            return (reg_value & self._mask) >> self._offset

        def __set__(self, obj, val):
            reg_value = obj._read_u8(self._address)
            reg_value &= ~self._mask
            reg_value |= (val & 0xFF) << self._offset
            obj._write_u8(self._address, reg_value)

    # Control bits from the registers of the chip:
    data_mode = _RegisterBits(_REG_DATA_MOD, offset=5, bits=2)
    modulation_type = _RegisterBits(_REG_DATA_MOD, offset=3, bits=2)
    modulation_shaping = _RegisterBits(_REG_DATA_MOD, offset=0, bits=2)
    temp_start = _RegisterBits(_REG_TEMP1, offset=3)
    temp_running = _RegisterBits(_REG_TEMP1, offset=2)
    sync_on = _RegisterBits(_REG_SYNC_CONFIG, offset=7)
    sync_size = _RegisterBits(_REG_SYNC_CONFIG, offset=3, bits=3)
    aes_on = _RegisterBits(_REG_PACKET_CONFIG2, offset=0)
    pa_0_on = _RegisterBits(_REG_PA_LEVEL, offset=7)
    pa_1_on = _RegisterBits(_REG_PA_LEVEL, offset=6)
    pa_2_on = _RegisterBits(_REG_PA_LEVEL, offset=5)
    output_power = _RegisterBits(_REG_PA_LEVEL, offset=0, bits=5)
    rx_bw_dcc_freq = _RegisterBits(_REG_RX_BW, offset=5, bits=3)
    rx_bw_mantissa = _RegisterBits(_REG_RX_BW, offset=3, bits=2)
    rx_bw_exponent = _RegisterBits(_REG_RX_BW, offset=0, bits=3)
    afc_bw_dcc_freq = _RegisterBits(_REG_AFC_BW, offset=5, bits=3)
    afc_bw_mantissa = _RegisterBits(_REG_AFC_BW, offset=3, bits=2)
    afc_bw_exponent = _RegisterBits(_REG_AFC_BW, offset=0, bits=3)
    packet_format = _RegisterBits(_REG_PACKET_CONFIG1, offset=7, bits=1)
    dc_free = _RegisterBits(_REG_PACKET_CONFIG1, offset=5, bits=2)
    crc_on = _RegisterBits(_REG_PACKET_CONFIG1, offset=4, bits=1)
    crc_auto_clear_off = _RegisterBits(_REG_PACKET_CONFIG1, offset=3, bits=1)
    address_filter = _RegisterBits(_REG_PACKET_CONFIG1, offset=1, bits=2)
    mode_ready = _RegisterBits(_REG_IRQ_FLAGS1, offset=7)
    rx_ready = _RegisterBits(_REG_IRQ_FLAGS1, offset=6)
    tx_ready = _RegisterBits(_REG_IRQ_FLAGS1, offset=5)
    dio_0_mapping = _RegisterBits(_REG_DIO_MAPPING1, offset=6, bits=2)
    packet_sent = _RegisterBits(_REG_IRQ_FLAGS2, offset=3)
    payload_ready = _RegisterBits(_REG_IRQ_FLAGS2, offset=2)


    def __init__(
        self,
        spi,
        cs,
        reset,
        dio0,
        frequency,
    ):
        self.spi = spi
        self.cs = cs
        self._reset = reset
        self.dio0 = dio0
        self.reset()  # Reset the chip.
        # Check the version of the chip.
        version = self._read_u8(_REG_VERSION)
        if version != 0x24:
            raise RuntimeError(
                "Failed to find RFM69 with expected version, check wiring!"
            )
        
        self._configure_radio_defaults()
        self.idle()  # Enter idle state.
        # Setup the chip in a similar way to the RadioHead RFM69 library.
        # Set FIFO TX condition to not empty and the default FIFO threshold to 15.
        self._write_u8(_REG_FIFO_THRESH, 0b10001111)
        # Configure low beta off.
        self._write_u8(_REG_TEST_DAGC, 0x30)
        # Disable boost.
        self._write_u8(_REG_TEST_PA1, _TEST_PA1_NORMAL)
        self._write_u8(_REG_TEST_PA2, _TEST_PA2_NORMAL)
        # Set the syncronization word.
        self.frequency_mhz = frequency  # Set frequency.

        # set radio configuration parameters
        self._configure_radio_defaults()

        self.running = False

        # initialize last RSSI reading
        self.last_rssi = 0.0

        self.tx_waiting_event = asyncio.ThreadSafeFlag()
        self.rx_received_event = asyncio.Event()
        self.rx_waiting_event = asyncio.ThreadSafeFlag()
        self.packet_sent_condition = asyncio.ThreadSafeFlag()

        self.module_io_lock = asyncio.Lock()

        self.rx_queue = deque([],100)
        self.tx_queue = deque([],100)

        dio0.irq(trigger=Pin.IRQ_RISING, handler= lambda p : self._dxo0_interrupt(p))

    def _configure_radio_defaults(self):
        # Configure modulation for RadioHead library GFSK_Rb250Fd250 mode
        # by default.  Users with advanced knowledge can manually reconfigure
        # for any other mode (consulting the datasheet is absolutely
        # necessary!).
        self.data_mode = 0b00  # Packet mode
        self.modulation_type = 0b00  # FSK modulation
        self.modulation_shaping = 0b01  # Gaussian filter, BT=1.0
        self.bitrate = 250000  # 250kbs
        self.frequency_deviation = 250000  # 250khz
        self.rx_bw_dcc_freq = 0b111  # RxBw register = 0xE0
        self.rx_bw_mantissa = 0b00
        self.rx_bw_exponent = 0b000
        self.afc_bw_dcc_freq = 0b111  # AfcBw register = 0xE0
        self.afc_bw_mantissa = 0b00
        self.afc_bw_exponent = 0b000
        self.packet_format = 1  # Variable length.
        self.dc_free = 0b10  # Whitening
        self.crc_on = 0  # CRC enabled
        self.crc_auto_clear = 0  # Clear FIFO on CRC fail
        self.address_filtering = 0b00  # No address filtering
        self.sync_word = b"\x2D\xD4"  # No sync word
        self.encryption_key = None
        self.preamble_length = 4 #No packet preamble
        # Set transmit power to 13 dBm, a safe value any module supports.
        self.high_power = True
        self.tx_power = 13

    # pylint: disable=no-member
    # Reconsider this disable when it can be tested.
    # read_into(_REG_FIFO, packet)
    def _read_into(self, address, buf):
        # Read from the specified address into the provided
        # buffer.
        self.cs.value(0)
        self.spi.write(bytes([address & 0x7F]))
        self.spi.readinto(buf)
        self.cs.value(1)
        return buf

    def _read_u8(self, address):
        # Read a single byte from the provided address and return it.
        self.cs.value(0)
        self.spi.write(bytes([address & 0x7F]))
        value = self.spi.read(1)
        self.cs.value(1)
        return value[0]

    def _write_from(self, address, buf):
        # Write to the provided address and taken from the
        # provided buffer.
        self.cs.value(0)
        self.spi.write(bytes([(address | 0x80) & 0xFF]))
        self.spi.write(buf)  # send data
        self.cs.value(1)

    def _write_fifo_from(self, buf):
        # Write to the transmit FIFO and taken from the
        # provided buffer.
        length = len(buf)
        buf1 = (_REG_FIFO | 0x80) & 0xFF  # Set top bit to 1 to
        # indicate a write.
        buf2 = length & 0xFF  # Set packt length
        self.cs.value(0)
        self.spi.write(bytes([buf1,buf2])) # send address and lenght)
        self.spi.write(buf)  # send data
        self.cs.value(1)

    def _write_u8(self, address, val):
        # Write a byte register to the chip.  Specify the 7-bit address and the
        # 8-bit value to write to that address.
        address = (address | 0x80) & 0xFF  # Set top bit to 1 to
        # indicate a write.
        val = val & 0xFF
        self.cs.value(0)
        self.spi.write(bytes([address,val]))
        self.cs.value(1)

    def reset(self):
        """Perform a reset of the chip."""
        # See section 7.2.2 of the datasheet for reset description.
        self._reset.value(1)
        time.sleep_us(100)  # 100 us
        self._reset.value(0)
        time.sleep_us(5000)  # 5 ms

    def idle(self):
        """Enter idle standby mode (switching off high power amplifiers if necessary)."""
        # Like RadioHead library, turn off high power boost if enabled.
        if self._tx_power >= 18:
            self._write_u8(_REG_TEST_PA1, _TEST_PA1_NORMAL)
            self._write_u8(_REG_TEST_PA2, _TEST_PA2_NORMAL)
        self.operation_mode = STANDBY_MODE

    def sleep(self):
        """Enter sleep mode."""
        self.operation_mode = SLEEP_MODE

    def listen(self):
        """Listen for packets to be received by the chip.  Use :py:func:`receive` to listen, wait
           and retrieve packets as they're available.
        """
        # Like RadioHead library, turn off high power boost if enabled.
        if self._tx_power >= 18:
            self._write_u8(_REG_TEST_PA1, _TEST_PA1_NORMAL)
            self._write_u8(_REG_TEST_PA2, _TEST_PA2_NORMAL)
        # Enable payload ready interrupt for D0 line.
        self.dio_0_mapping = 0b01
        # Enter RX mode (will clear FIFO!).
        self.operation_mode = RX_MODE

    def transmit(self):
        """Transmit a packet which is queued in the FIFO.  This is a low level function for
           entering transmit mode and more.  For generating and transmitting a packet of data use
           :py:func:`send` instead.
        """
        # Like RadioHead library, turn on high power boost if enabled.
        if self._tx_power >= 18:
            self._write_u8(_REG_TEST_PA1, _TEST_PA1_BOOST)
            self._write_u8(_REG_TEST_PA2, _TEST_PA2_BOOST)
        # Enable packet sent interrupt for D0 line.
        self.dio_0_mapping = 0b00
        # Enter TX mode (will clear FIFO!).
        self.operation_mode = TX_MODE

    @property
    def is_high_power(self):
        return self.high_power
    
    @is_high_power.setter
    def is_high_power(self, val):
        self.high_power = val

    @property
    def temperature(self):
        """The internal temperature of the chip in degrees Celsius. Be warned this is not
           calibrated or very accurate.

           .. warning:: Reading this will STOP any receiving/sending that might be happening!
        """
        # Start a measurement then poll the measurement finished bit.
        self.temp_start = 1
        while self.temp_running > 0:
            pass
        # Grab the temperature value and convert it to Celsius.
        # This uses the same observed value formula from the Radiohead library.
        temp = self._read_u8(_REG_TEMP2)
        return 166.0 - temp

    @property
    def operation_mode(self):
        """The operation mode value.  Unless you're manually controlling the chip you shouldn't
           change the operation_mode with this property as other side-effects are required for
           changing logical modes--use :py:func:`idle`, :py:func:`sleep`, :py:func:`transmit`,
           :py:func:`listen` instead to signal intent for explicit logical modes.
        """
        op_mode = self._read_u8(_REG_OP_MODE)
        calculated_opMode =  (op_mode >> 2) & 0b111
        return calculated_opMode
    

    @operation_mode.setter
    def operation_mode(self, val):
        assert 0 <= val <= 4
        # Set the mode bits inside the operation mode register.
        op_mode = self._read_u8(_REG_OP_MODE)
        op_mode &= 0b11100011
        op_mode |= val << 2
        self._write_u8(_REG_OP_MODE, op_mode)
        # Wait for mode to change by polling interrupt bit.
        while not self.mode_ready:
            pass

    @property
    def sync_word(self):
        """The synchronization word value.  This is a byte string up to 8 bytes long (64 bits)
           which indicates the synchronization word for transmitted and received packets. Any
           received packet which does not include this sync word will be ignored. The default value
           is 0x2D, 0xD4 which matches the RadioHead RFM69 library. Setting a value of None will
           disable synchronization word matching entirely.
        """
        # Handle when sync word is disabled..
        if not self.sync_on:
            return None
        # Sync word is not disabled so read the current value.
        sync_word_length = self.sync_size + 1  # Sync word size is offset by 1
        # according to datasheet.
        sync_word = bytearray(sync_word_length)
        self._read_into(_REG_SYNC_VALUE1, sync_word)
        return sync_word

    @sync_word.setter
    def sync_word(self, val):
        # Handle disabling sync word when None value is set.
        if val is None:
            self.sync_on = 0
        else:
            # Check sync word is at most 8 bytes.
            assert 1 <= len(val) <= 8
            # Update the value, size and turn on the sync word.
            self._write_from(_REG_SYNC_VALUE1, val)
            self.sync_size = len(val) - 1  # Again sync word size is offset by
            # 1 according to datasheet.
            self.sync_on = 1

    @property
    def preamble_length(self):
        """The length of the preamble for sent and received packets, an unsigned 16-bit value.
           Received packets must match this length or they are ignored! Set to 4 to match the
           RadioHead RFM69 library.
        """
        msb = self._read_u8(_REG_PREAMBLE_MSB)
        lsb = self._read_u8(_REG_PREAMBLE_LSB)
        return ((msb << 8) | lsb) & 0xFFFF

    @preamble_length.setter
    def preamble_length(self, val):
        assert 0 <= val <= 65535
        self._write_u8(_REG_PREAMBLE_MSB, (val >> 8) & 0xFF)
        self._write_u8(_REG_PREAMBLE_LSB, val & 0xFF)

    @property
    def frequency_mhz(self):
        """The frequency of the radio in Megahertz. Only the allowed values for your radio must be
           specified (i.e. 433 vs. 915 mhz)!
        """
        # FRF register is computed from the frequency following the datasheet.
        # See section 6.2 and FRF register description.
        # Read bytes of FRF register and assemble into a 24-bit unsigned value.
        msb = self._read_u8(_REG_FRF_MSB)
        mid = self._read_u8(_REG_FRF_MID)
        lsb = self._read_u8(_REG_FRF_LSB)
        frf = ((msb << 16) | (mid << 8) | lsb) & 0xFFFFFF
        frequency = (frf * _FSTEP) / 1000000.0
        return frequency

    @frequency_mhz.setter
    def frequency_mhz(self, val):
        assert 290 <= val <= 1020
        # Calculate FRF register 24-bit value using section 6.2 of the datasheet.
        frf = int((val * 1000000.0) / _FSTEP) & 0xFFFFFF
        # Extract byte values and update registers.
        msb = frf >> 16
        mid = (frf >> 8) & 0xFF
        lsb = frf & 0xFF
        self._write_u8(_REG_FRF_MSB, msb)
        self._write_u8(_REG_FRF_MID, mid)
        self._write_u8(_REG_FRF_LSB, lsb)

    @property
    def encryption_key(self):
        """The AES encryption key used to encrypt and decrypt packets by the chip. This can be set
           to None to disable encryption (the default), otherwise it must be a 16 byte long byte
           string which defines the key (both the transmitter and receiver must use the same key
           value).
        """
        # Handle if encryption is disabled.
        if self.aes_on == 0:
            return None
        # Encryption is enabled so read the key and return it.
        key = bytearray(16)
        self._read_into(_REG_AES_KEY1, key)
        return key

    @encryption_key.setter
    def encryption_key(self, val):
        # Handle if unsetting the encryption key (None value).
        if val is None:
            self.aes_on = 0
        else:
            # Set the encryption key and enable encryption.
            assert len(val) == 16
            self._write_from(_REG_AES_KEY1, val)
            self.aes_on = 1

    @property
    def tx_power(self):
        """The transmit power in dBm. Can be set to a value from -2 to 20 for high power devices
           (RFM69HCW, high_power=True) or -18 to 13 for low power devices. Only integer power
           levels are actually set (i.e. 12.5 will result in a value of 12 dBm).
        """
        # Follow table 10 truth table from the datasheet for determining power
        # level from the individual PA level bits and output power register.
        pa0 = self.pa_0_on
        pa1 = self.pa_1_on
        pa2 = self.pa_2_on
        if pa0 and not pa1 and not pa2:
            # -18 to 13 dBm range
            return -18 + self.output_power
        if not pa0 and pa1 and not pa2:
            # -2 to 13 dBm range
            return -18 + self.output_power
        if not pa0 and pa1 and pa2 and not self.high_power:
            # 2 to 17 dBm range
            return -14 + self.output_power
        if not pa0 and pa1 and pa2 and self.high_power:
            # 5 to 20 dBm range
            return -11 + self.output_power
        raise RuntimeError("Power amplifiers in unknown state!")

    @tx_power.setter
    def tx_power(self, val):
        val = int(val)
        # Determine power amplifier and output power values depending on
        # high power state and requested power.
        pa_0_on = 0
        pa_1_on = 0
        pa_2_on = 0
        output_power = 0
        if self.high_power:
            # Handle high power mode.
            assert -2 <= val <= 20
            if val <= 13:
                pa_1_on = 1
                output_power = val + 18
            elif 13 < val <= 17:
                pa_1_on = 1
                pa_2_on = 1
                output_power = val + 14
            else:  # power >= 18 dBm
                # Note this also needs PA boost enabled separately!
                pa_1_on = 1
                pa_2_on = 1
                output_power = val + 11
        else:
            # Handle non-high power mode.
            assert -18 <= val <= 13
            # Enable only power amplifier 0 and set output power.
            pa_0_on = 1
            output_power = val + 18
        # Set power amplifiers and output power as computed above.
        self.pa_0_on = pa_0_on
        self.pa_1_on = pa_1_on
        self.pa_2_on = pa_2_on
        self.output_power = output_power
        self._tx_power = val

    @property
    def rssi(self):
        """The received strength indicator (in dBm).
           May be inaccuate if not read immediatey. last_rssi contains the value read immediately
           receipt of the last packet.
        """
        # Read RSSI register and convert to value using formula in datasheet.
        return -self._read_u8(_REG_RSSI_VALUE) / 2.0

    @property
    def bitrate(self):
        """The modulation bitrate in bits/second (or chip rate if Manchester encoding is enabled).
           Can be a value from ~489 to 32mbit/s, but see the datasheet for the exact supported
           values.
        """
        msb = self._read_u8(_REG_BITRATE_MSB)
        lsb = self._read_u8(_REG_BITRATE_LSB)
        return _FXOSC / ((msb << 8) | lsb)

    @bitrate.setter
    def bitrate(self, val):
        assert (_FXOSC / 65535) <= val <= 32000000.0
        # Round up to the next closest bit-rate value with addition of 0.5.
        bitrate = int((_FXOSC / val) + 0.5) & 0xFFFF
        self._write_u8(_REG_BITRATE_MSB, bitrate >> 8)
        self._write_u8(_REG_BITRATE_LSB, bitrate & 0xFF)

    @property
    def frequency_deviation(self):
        """The frequency deviation in Hertz."""
        msb = self._read_u8(_REG_FDEV_MSB)
        lsb = self._read_u8(_REG_FDEV_LSB)
        return _FSTEP * ((msb << 8) | lsb)

    @frequency_deviation.setter
    def frequency_deviation(self, val):
        assert 0 <= val <= (_FSTEP * 16383)  # fdev is a 14-bit unsigned value
        # Round up to the next closest integer value with addition of 0.5.
        fdev = int((val / _FSTEP) + 0.5) & 0x3FFF
        self._write_u8(_REG_FDEV_MSB, fdev >> 8)
        self._write_u8(_REG_FDEV_LSB, fdev & 0xFF)

    def _transmitfifo(self, data):
        self._write_fifo_from(data)
        self.transmit()
        
    def _readfifo(self):

        fifo_length = self._read_u8(_REG_FIFO)
        packet = bytearray(fifo_length)
        self._read_into(_REG_FIFO, packet)

        return packet
        
    def _dxo0_interrupt(self,pin):
        try:
            dio0mapping = self.dio_0_mapping
            if(self.operation_mode == RX_MODE):
                if(dio0mapping == _DIO_MAPPING_DIO0_RX_PAYLOADREADY):
                    self.rx_waiting_event.set()
            elif(self.operation_mode == TX_MODE):
                if(dio0mapping == _DIO_MAPPING_DIO0_TX_PACKETSENT):
                    self.packet_sent_condition.set()
        except Exception as e:
            print("Interrupt error: ", e)

    def get_rx_packet(self):
        return self.rx_queue.popleft()
    
    def _get_tx_packet(self):
        return self.tx_queue.popleft()
    
    def send(self,data):
        self.tx_queue.append(data)
        self.tx_waiting_event.set()
    
    def start(self):
        self.running = True
        self.listen()
        asyncio.create_task(self._receive_rx())
        asyncio.create_task(self._handle_tx())

    def stop(self):
        self.running = False
    
    async def wait_for_rx(self):
        await self.rx_received_event.wait()
        data = self.get_rx_packet()
        self.rx_received_event.clear()
        if(len(self.rx_queue) > 0):
            self.rx_received_event.set()
        return data

    async def _receive_rx(self):
        while(self.running):
            try:
                await self.rx_waiting_event.wait()
                await self.module_io_lock.acquire()      
                data = self._readfifo()
                self.rx_queue.append(data)
                self.rx_waiting_event.clear()
                self.rx_received_event.set()
            except Exception as e:
                print("receive error: ", e)
            finally:
                self.module_io_lock.release()


    async def _handle_tx(self):
        try:
            while self.running:
                try:
                    await self.tx_waiting_event.wait()
                    try:
                        await self.module_io_lock.acquire()
                        self.idle()
                        # Transmit all packets in the queue, be aware that if the queue is filled alot then nothing else happens on the thread until all packets are sent
                        # sending them all at once removes the need to switch back and forth between modes on the radio but could also cause received packets to be lost.
                        while(len(self.tx_queue) > 0):
                            self._transmitfifo(self._get_tx_packet())
                            self.transmit()
                            await self.packet_sent_condition.wait()
                            self.packet_sent_condition.clear()
                        self.listen()
                    finally:
                        self.module_io_lock.release()
                    
                    if(len(self.tx_queue) == 0):
                        self.tx_waiting_event.clear()

                except Exception as e:
                    print("Handle Tx Error: ", e)
        except Exception as e:
            print("Handle Tx Error: ", e)







    

