import time
from devices import rx5808
from adafruit import RFM69
from machine import Pin, SPI

try:
   print("Initializing Devices")
   CS = Pin(21, Pin.OUT)
   RESET = Pin(18, Pin.OUT)
   spi = SPI(1, baudrate=1_000_000, polarity=0, phase=0, bits=8, firstbit= SPI.MSB, sck=Pin(48), mosi=Pin(38), miso=Pin(47))

   RADIO_FREQ_MHZ = 433.0

   rfm69 = None

   while rfm69 == None:
      try:
         rfm69 = RFM69(spi, CS, RESET, RADIO_FREQ_MHZ)
      except Exception as e:
         time.sleep(1)
         print("FAILED TO INITIALIZE RFM69: " + repr(e))

   receiver_group = rx5808.Rx5808RegisterCommunication(6,5,9)

   start_frequency = 5650
   end_frequency = 5950
   frequency_increment = 1
   current_frequency = start_frequency

   current_device = 0

   while True:
      try:
         print("Tuning: " + str(current_frequency) + " MHz")
         timebefore = time.ticks_us()
         tunedFrequency = receiver_group.set_frequency(current_frequency)
         tuned_frequency = receiver_group.get_stored_frequency()
         timetotune = time.ticks_us()
         print("Time to tune: " + str(timetotune - timebefore) + " us")
         print("Tuned frequency: " + str(tuned_frequency) + " MHz")
                     
         current_frequency += frequency_increment
         if current_frequency > end_frequency:
            current_frequency = start_frequency
         
         timebefore = time.ticks_us()
         rfm69.send(str(tuned_frequency))
         tunetosend = time.ticks_us()
         print("Time to send: " + str(tunetosend - timebefore) + " us")
      except Exception as e:
         print("An error occurred "+ repr(e))
         time.sleep_ms(1000)

except Exception as e:
   print("An error occurred "+ repr(e))