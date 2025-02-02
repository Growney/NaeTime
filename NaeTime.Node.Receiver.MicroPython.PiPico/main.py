import time
import asyncio
import sys
import uselect
from adafruit import RFM69
from machine import Pin, SPI
from collections import deque

START_OF_RECORD = 0x5a
END_OF_RECORD = 0x5b
ESCAPE = 0x5c
ESCAPED_ADDER = 0x40

print("Starting device")

CS = 21
RESET = 18
DIO0 = 5
SCK = 48
MOSI = 38
MISO = 47

spi = SPI(1, baudrate=1_000_000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(SCK), mosi=Pin(MOSI), miso=Pin(MISO))
RADIO_FREQ_MHZ = 433.0
chip_select_pin = Pin(CS, Pin.OUT)
reset_pin = Pin(RESET, Pin.OUT)
dio0_pin = Pin(DIO0, Pin.IN)

receiver = RFM69(spi,chip_select_pin, reset_pin, dio0_pin, RADIO_FREQ_MHZ)
receiver.sync_on = True
receiver.sync_word = "NaeTime"


async def tx_task(rfm69):
   print("Starting tx task")
   spoll = uselect.poll()
   spoll.register(sys.stdin, uselect.POLLIN)

   tx_queue = deque([],100)
   previous_byte_escape = False
   inrecord = False
   while True:
      try:
         await asyncio.sleep_ms(20)
         while(spoll.poll(0)):
            byte = sys.stdin.buffer.read(1)[0]
            if(byte == END_OF_RECORD):
               inrecord = False
               packet = bytearray()
               while(len(tx_queue) > 0):
                  packet.append(tx_queue.popleft())
               await rfm69.send(packet)
            elif(byte == ESCAPE):
               previous_byte_escape = True
            elif(byte == START_OF_RECORD):
               inrecord = True;
               while(len(tx_queue) > 0):
                  tx_queue.popleft()
            elif(inrecord):
               if(previous_byte_escape):
                  byte = byte - ESCAPED_ADDER
                  previous_byte_escape = False 
               tx_queue.append(byte)
      except Exception:
         print("Exception in tx_task")


async def main_process(rfm69):
   print("Starting main process")
   rfm69.start()
   asyncio.create_task(tx_task(rfm69))
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
