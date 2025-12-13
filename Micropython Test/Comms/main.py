import network
import time
import machine
from machine import UART, Pin

uart = UART(4, baudrate=57600 , tx=Pin(17), rx=Pin(18))  # Example pins for ESP32

lan=network.LAN(mdc=machine.Pin(31), mdio=machine.Pin(52),
    phy_type=network.PHY_IP101, phy_addr=1, reset=machine.Pin(51),
    ref_clk_mode=machine.Pin.IN, ref_clk=machine.Pin(50))
lan.active(True)
counter = 0
while True:
    msg = str(lan.isconnected()) +"," + str(lan.ipconfig("addr4")[0])
    uart.write(msg)
    print("Sent:", msg)
    counter += 1
    time.sleep(1)