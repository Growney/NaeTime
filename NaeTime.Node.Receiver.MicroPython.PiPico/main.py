import time
import asyncio
import sys
from adafruit import RFM69
from machine import Pin, SPI
from collections import deque

START_OF_RECORD = 0x5a
END_OF_RECORD = 0x5b
ESCAPE = 0x5c
ESCAPED_ADDER = 0x40

print("Starting device")

CS = Pin(5, Pin.OUT)
RESET = Pin(6, Pin.OUT)
DIO0 = Pin(7, Pin.IN)
spi = SPI(0, baudrate=1000000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(2), mosi=Pin(3), miso=Pin(4))
RADIO_FREQ_MHZ = 433.0

receiver = RFM69(spi,CS, RESET, DIO0, RADIO_FREQ_MHZ)
receiver.sync_on = True
receiver.sync_word = "NaeTime"


async def tx_task(rfm69):
   print("Starting tx task")
   tx_queue = deque(100)
   in_record = False
   previous_byte_escape = False
   while True:
      await asyncio.sleep_ms(500)
      while(sys.stdin.readable()):
         data = sys.stdin.read()
         if(data == END_OF_RECORD):
            packet = bytearray()
            while(len(tx_queue) > 0):
               packet.append(tx_queue.popleft())
            await rfm69.send(packet)
         elif(data == ESCAPE):
            previous_byte_escape = True
         else:
            if(previous_byte_escape):
               data = data - ESCAPED_ADDER
               previous_byte_escape = False  
            else:
               data = data
            tx_queue.append(data)

async def main_process(rfm69):
   print("Starting main process")
   rfm69.start()
   while True:
      packet = await rfm69.wait_for_rx()
      data = bytearray()
      data.append(START_OF_RECORD)
      for byte in packet:
         if(byte == START_OF_RECORD or byte == END_OF_RECORD or byte == ESCAPE):
            data.append(ESCAPE)
            data.append(byte + ESCAPED_ADDER)
         else:
            data.append(byte)
      data.append(END_OF_RECORD)
      sys.stdout.write(data)

asyncio.run(main_process(receiver))
