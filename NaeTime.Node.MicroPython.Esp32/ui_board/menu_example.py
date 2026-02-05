# menu_example.py
# Example usage of the Menu class with TFT display and D-pad

from machine import Pin, SPI
import time
import ST7735
import font5x8
from dpad import DPad
from menu import Menu

# --- SPI and Pin Setup ---
spi = SPI(1, baudrate=20000000, polarity=0, phase=0,
          sck=Pin(19), mosi=Pin(21))

# Control pins
cs_pin = 4
rst_pin = 15
dc_pin = 2

# Initialize display
tft = ST7735.TFT(spi, dc_pin, rst_pin, cs_pin)
tft.initr()
tft.rotation(3)
tft.fill(ST7735.TFT.BLACK)

# Initialize D-pad
dpad = DPad(up_pin=25, down_pin=26, left_pin=27, right_pin=14)

# Create menu
menu = Menu(tft, dpad, title="Main Menu")

# --- Define callbacks for menu items ---
def option1_callback():
    """Example callback for Option 1."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), "Option 1 Selected!", ST7735.TFT.GREEN, font5x8.font5x8)
    time.sleep(1)
    menu.show()  # Return to menu

def option2_callback():
    """Example callback for Option 2."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), "Option 2 Selected!", ST7735.TFT.CYAN, font5x8.font5x8)
    time.sleep(1)
    menu.show()  # Return to menu

def settings_callback():
    """Example callback for Settings."""
    # Create a submenu
    submenu = Menu(tft, dpad, title="Settings")
    submenu.add_item("Display", lambda: show_message("Display Settings"))
    submenu.add_item("Sound", lambda: show_message("Sound Settings"))
    submenu.add_item("Back", lambda: menu.show())
    submenu.show()
    
    # Run submenu loop
    while True:
        if submenu.update():
            # Check if "Back" was selected
            selected = submenu.get_selected_item()
            if selected.label == "Back":
                break
        time.sleep(0.05)

def show_message(msg):
    """Helper to show a message and return to menu."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), msg, ST7735.TFT.WHITE, font5x8.font5x8)
    time.sleep(1)
    menu.show()

def about_callback():
    """Show about information."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 30), "NaeTime UI Board", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 45), "Version 1.0", ST7735.TFT.CYAN, font5x8.font5x8)
    tft.text((10, 60), "ESP32 MicroPython", ST7735.TFT.GREEN, font5x8.font5x8)
    tft.text((10, 90), "Press RIGHT to go back", ST7735.TFT.GRAY, font5x8.font5x8)
    
    # Wait for button press
    while True:
        state = dpad.read()
        if state["right"]["state"] == "button_down":
            break
        time.sleep(0.05)
    
    menu.show()  # Return to menu

def exit_callback():
    """Exit menu and clear screen."""
    menu.hide()
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), "Menu Exited", ST7735.TFT.RED, font5x8.font5x8)

# --- Add menu items ---
menu.add_item("Start Race", option1_callback)
menu.add_item("View Results", option2_callback)
menu.add_item("Settings", settings_callback)
menu.add_item("Calibrate", lambda: show_message("Calibrating..."))
menu.add_item("Network Info", lambda: show_message("Network: Connected"))
menu.add_item("About", about_callback)
menu.add_item("Exit", exit_callback)

# Show the menu
menu.show()

print("Menu system started!")

# --- Main loop ---
try:
    while True:
        menu.update()
        time.sleep(0.05)  # 50ms update rate
        
except KeyboardInterrupt:
    print("Menu system stopped")
    tft.fill(ST7735.TFT.BLACK)
