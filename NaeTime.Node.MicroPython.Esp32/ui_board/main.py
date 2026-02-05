# main.py
from machine import Pin, SPI, UART
import time
import ST7735  # Your driver file
import font5x8  # Optional font module if you have one
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

# Use red tab init (you can change to initb / initg if your display tab is different)
tft.initr()
tft.rotation(3)
tft.fill(ST7735.TFT.BLACK)

# Initialize D-pad with pin assignments
# up=19, down=16, left=18, right=17
dpad = DPad(up_pin=25, down_pin=26, left_pin=27, right_pin=14)

print("D-Pad button state monitor started!")

# Track previous states to detect changes per button
previous_states = None

# Y position for each button
button_positions = {
    "up": 20,
    "down": 45,
    "left": 70,
    "right": 95
}

def draw_button_state(button_name, y_pos, state, held_ms):
    """Draw a single button's state at the specified y position."""
    # Clear the button's area (2 lines high for state + held time)
    tft.fillrect((5, y_pos), (150, 20), ST7735.TFT.BLACK)
    
    # Choose color based on state
    if state == "button_down":
        color = ST7735.TFT.GREEN
    elif state == "pressed":
        color = ST7735.TFT.YELLOW
    elif state == "button_up":
        color = ST7735.TFT.CYAN
    else:  # unpressed
        color = ST7735.TFT.WHITE
    
    # Display button name and state
    label = f"{button_name.upper():5}: {state:12}"
    tft.text((5, y_pos), label, color, font5x8.font5x8)
    
    # Display held time if button is pressed
    if held_ms > 0:
        time_label = f"{held_ms}ms"
        tft.text((5, y_pos + 10), time_label, ST7735.TFT.RED, font5x8.font5x8)

def update_held_time(y_pos, held_ms):
    """Update only the held time display without redrawing the label."""
    # Clear just the time area
    tft.fillrect((5, y_pos + 10), (150, 10), ST7735.TFT.BLACK)
    
    # Display updated held time
    if held_ms > 0:
        time_label = f"{held_ms}ms"
        tft.text((5, y_pos + 10), time_label, ST7735.TFT.RED, font5x8.font5x8)


# Main loop to read and display button states
def update_dpad_states(tft, dpad, previous_states, button_positions):
    states = dpad.read()
    # First iteration: draw title and all buttons
    if previous_states is None:
        tft.text((10, 5), "D-Pad Status", ST7735.TFT.WHITE, font5x8.font5x8)
        for button_name in ["up", "down", "left", "right"]:
            y_pos = button_positions[button_name]
            current = states[button_name]
            draw_button_state(button_name, y_pos, current["state"], current["held_ms"])
        previous_states = {name: data.copy() for name, data in states.items()}
    else:
        # Subsequent iterations: only redraw changed buttons
        for button_name in ["up", "down", "left", "right"]:
            current = states[button_name]
            previous = previous_states[button_name]
            
            # Check if this specific button's state changed
            if current["state"] != previous["state"]:
                # Redraw only this button's area
                y_pos = button_positions[button_name]
                draw_button_state(button_name, y_pos, current["state"], current["held_ms"])
                
                # Update previous state for this button
                previous_states[button_name] = current.copy()
            
            # If button is in pressed state and held time changed, update only the time
            elif current["state"] == "pressed" and current["held_ms"] != previous["held_ms"]:
                y_pos = button_positions[button_name]
                update_held_time(y_pos, current["held_ms"])
                
                # Update previous held time for this button
                previous_states[button_name]["held_ms"] = current["held_ms"]
    return previous_states

red = Pin(22, Pin.OUT)
green = Pin(32, Pin.OUT)
blue = Pin(33, Pin.OUT)

red.value(1)
green.value(3)
blue.value(3)

# --- Menu System Setup ---

# Menu callbacks
def show_dpad_status():
    """Show D-pad button state monitor."""
    global previous_states
    tft.fill(ST7735.TFT.BLACK)
    previous_states = None
    
    # Double tap detection for left button
    tap_count = 0
    first_tap_time = 0
    double_tap_window = 800  # ms window for double tap
    waiting_for_release = False
    
    # Run D-pad monitor until left button is double-tapped to go back
    while True:
        previous_states = update_dpad_states(tft, dpad, previous_states, button_positions)
        
        # Check for left button double tap to exit back to menu
        state = dpad.read()
        left_state = state["left"]["state"]
        
        if left_state == "button_down":
            if not waiting_for_release:
                current_time = time.ticks_ms()
                if tap_count == 0:
                    # First tap
                    tap_count = 1
                    first_tap_time = current_time
                    waiting_for_release = True
                elif tap_count == 1 and time.ticks_diff(current_time, first_tap_time) < double_tap_window:
                    # Second tap within window - double tap detected!
                    menu.show()
                    break
                else:
                    # Too slow, reset
                    tap_count = 1
                    first_tap_time = current_time
                    waiting_for_release = True
        
        elif left_state == "unpressed":
            # Button released
            if waiting_for_release:
                waiting_for_release = False
            # Reset if window expired
            if tap_count > 0 and time.ticks_diff(time.ticks_ms(), first_tap_time) >= double_tap_window:
                tap_count = 0
        
        time.sleep(0.05)

def start_race():
    """Start race sequence."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), "Starting Race...", ST7735.TFT.GREEN, font5x8.font5x8)
    time.sleep(1)
    tft.text((10, 65), "3...", ST7735.TFT.YELLOW, font5x8.font5x8)
    time.sleep(1)
    tft.text((10, 80), "2...", ST7735.TFT.YELLOW, font5x8.font5x8)
    time.sleep(1)
    tft.text((10, 95), "1...", ST7735.TFT.YELLOW, font5x8.font5x8)
    time.sleep(1)
    tft.text((10, 110), "GO!", ST7735.TFT.RED, font5x8.font5x8)
    time.sleep(2)
    menu.show()

def view_results():
    """View race results."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 30), "Race Results", ST7735.TFT.CYAN, font5x8.font5x8)
    tft.text((10, 50), "1st: 12.34s", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 65), "2nd: 12.89s", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 80), "3rd: 13.12s", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 105), "Press RIGHT to return", ST7735.TFT.GRAY, font5x8.font5x8)
    
    # Wait for button press
    while True:
        state = dpad.read()
        if state["right"]["state"] == "button_down" or state["left"]["state"] == "button_down":
            break
        time.sleep(0.05)
    menu.show()

def calibrate():
    """Calibration routine."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), "Calibrating...", ST7735.TFT.YELLOW, font5x8.font5x8)
    time.sleep(2)
    tft.text((10, 70), "Complete!", ST7735.TFT.GREEN, font5x8.font5x8)
    time.sleep(1)
    menu.show()

def network_info():
    """Display network information."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 30), "Network Info", ST7735.TFT.CYAN, font5x8.font5x8)
    tft.text((10, 50), "Status: Connected", ST7735.TFT.GREEN, font5x8.font5x8)
    tft.text((10, 65), "Nodes: 4", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 80), "Signal: Good", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 105), "Press RIGHT to return", ST7735.TFT.GRAY, font5x8.font5x8)
    
    # Wait for button press
    while True:
        state = dpad.read()
        if state["right"]["state"] == "button_down" or state["left"]["state"] == "button_down":
            break
        time.sleep(0.05)
    menu.show()

def about():
    """Show about information."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 30), "NaeTime UI", ST7735.TFT.WHITE, font5x8.font5x8)
    tft.text((10, 45), "Version 1.0", ST7735.TFT.CYAN, font5x8.font5x8)
    tft.text((10, 60), "ESP32 MicroPython", ST7735.TFT.GREEN, font5x8.font5x8)
    tft.text((10, 90), "Press LEFT to return", ST7735.TFT.GRAY, font5x8.font5x8)
    
    # Wait for button press
    while True:
        state = dpad.read()
        if state["right"]["state"] == "button_down" or state["left"]["state"] == "button_down":
            break
        time.sleep(0.05)
    menu.show()

def set_led(color_name, value):
    """Set LED value."""
    if color_name == "red":
        red.value(value)
    elif color_name == "green":
        green.value(value)
    elif color_name == "blue":
        blue.value(value)
    
    tft.fill(ST7735.TFT.BLACK)
    state = "ON" if value else "OFF"
    tft.text((10, 50), f"{color_name.upper()} LED {state}", ST7735.TFT.WHITE, font5x8.font5x8)
    time.sleep(1)

# Create Settings submenu
settings_menu = Menu(tft, dpad, title="Settings")

led_sub_menu = Menu(tft, dpad, title="LED Control", parent_menu=settings_menu)
led_sub_menu.add_item("Red ON", lambda: set_led("red", 1))
led_sub_menu.add_item("Red OFF", lambda: set_led("red", 0))
led_sub_menu.add_item("Green ON", lambda: set_led("green", 1))

# Create LED Control submenu
led_menu = Menu(tft, dpad, title="LED Control", parent_menu=settings_menu)
led_menu.add_item("Red ON", lambda: set_led("red", 1))
led_menu.add_item("Red OFF", lambda: set_led("red", 0))
led_menu.add_item("Green ON", lambda: set_led("green", 1))
led_menu.add_item("Green OFF", lambda: set_led("green", 0))
led_menu.add_item("Blue ON", lambda: set_led("blue", 1))
led_menu.add_submenu("More",led_sub_menu)
led_menu.add_back_item()

# Add items to settings menu
settings_menu.add_submenu("LED Control", led_menu)
settings_menu.add_item("Display Brightness", lambda: show_temp_msg("Brightness: 100%"))
settings_menu.add_item("Sound Volume", lambda: show_temp_msg("Volume: 80%"))
settings_menu.add_back_item()



def show_temp_msg(msg):
    """Show temporary message and return to settings."""
    tft.fill(ST7735.TFT.BLACK)
    tft.text((10, 50), msg, ST7735.TFT.WHITE, font5x8.font5x8)
    time.sleep(1)
    settings_menu.show()

# Create and configure main menu
menu = Menu(tft, dpad, title="NaeTime Menu")
menu.add_item("Start Race", start_race)
menu.add_item("View Results", view_results)
menu.add_submenu("Settings", settings_menu)
menu.add_item("Calibrate", calibrate)
menu.add_item("Network Info", network_info)
menu.add_item("DPad Status", show_dpad_status)
menu.add_item("About", about)

# Show the menu
menu.show()

print("NaeTime Menu System Started!")

pin = Pin(23, Pin.OUT)
pin.value(1)

# Main loop
while True:
    menu.update()
    time.sleep(0.05)


# async def sound_buzzer(pattern):
#     global buzzer_pin
#     for step in pattern:

#         buzzer_pin.value(step[0])
#         await asyncio.sleep_ms(step[1])
    
#     buzzer_pin.value(0)