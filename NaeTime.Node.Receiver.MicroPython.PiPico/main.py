import time
import sys
import asyncio
from adafruit import RFM69
from machine import Pin, SPI

print("Starting device")

CS = Pin(5, Pin.OUT)
RESET = Pin(6, Pin.OUT)
DIO0 = Pin(7, Pin.IN)
spi = SPI(0, baudrate=1000000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(2), mosi=Pin(3), miso=Pin(4))
RADIO_FREQ_MHZ = 433.0


async def process_packets(rfm69):
   print("Starting packet processing")
   while True:
      try:
         print("before wait for rx")
         packet = await rfm69.wait_for_rx()
         print("Received: ", packet)
      except Exception as e:
         print("Error: ", e)
   

async def main_process(rfm69):
   print("Starting main process")
   rfm69.start()
   await process_packets(rfm69)
   print("after packet processing")
   while True:
      print("Spinning in main loop")
      await asyncio.sleep(1)


time.sleep(2)
asyncio.run(main_process(RFM69(spi, CS, RESET, DIO0, RADIO_FREQ_MHZ)))
