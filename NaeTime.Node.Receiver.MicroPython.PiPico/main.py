import time
import asyncio
import sys
from adafruit import RFM69
from machine import Pin, SPI
from collections import deque

print("Starting device")

CS = Pin(5, Pin.OUT)
RESET = Pin(6, Pin.OUT)
DIO0 = Pin(7, Pin.IN)
spi = SPI(0, baudrate=1000000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(2), mosi=Pin(3), miso=Pin(4))
RADIO_FREQ_MHZ = 433.0

receiver = RFM69(spi,CS, RESET, DIO0, RADIO_FREQ_MHZ)
receiver.sync_on = True
receiver.sync_word = "NaeTime"

rx_queue = deque([],100)

async def process_packets(rfm69):
   global rx_queue
   while True:
      print("Waiting for packet")
      try:
         packet = await rfm69.wait_for_rx()
         rx_queue.append(packet)
      except Exception as e:
         print("Error: ", e)


async def main_process(rfm69):
   print("Starting main process")
   global rx_queue
   rfm69.start()
   await process_packets(rfm69)
   while True:
      while(len(rx_queue) > 0):
         sys.stdout.write(rx_queue.popleft())
         print("here")


asyncio.run(main_process(receiver))
