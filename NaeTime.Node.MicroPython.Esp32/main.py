import machine
import time

red = machine.Pin(46, machine.Pin.OUT)
blue = machine.Pin(45, machine.Pin.OUT)
green = machine.Pin(0, machine.Pin.OUT)

while True:
   red.value(0)
   time.sleep_us(100000)
   red.value(1)
   time.sleep_us(100000)
   blue.value(0)
   time.sleep_us(100000)
   blue.value(1)
   time.sleep_us(100000)
   green.value(0)
   time.sleep_us(100000)
   green.value(1)
   time.sleep_us(100000)