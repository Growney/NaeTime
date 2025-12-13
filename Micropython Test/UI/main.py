# main.py
from machine import Pin, SPI, UART
import time
import ST7735  # Your driver file
import font5x8  # Optional font module if you have one
from dpad import DPad

# --- SPI and Pin Setup ---
spi = SPI(1, baudrate=20000000, polarity=0, phase=0,
          sck=Pin(18), mosi=Pin(23))

# Control pins
cs_pin = 27
rst_pin = 25
dc_pin = 26

# Initialize display
tft = ST7735.TFT(spi, dc_pin, rst_pin, cs_pin)

# Use red tab init (you can change to initb / initg if your display tab is different)
tft.initr()
tft.rotation(3)
tft.fill(ST7735.TFT.BLACK)

# Initialize D-pad with pin assignments
# up=19, down=16, left=18, right=17
dpad = DPad(up_pin=19, down_pin=33, left_pin=21, right_pin=32)

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

uart = UART(1, baudrate=57600 , tx=Pin(13), rx=Pin(12))

# Track previous LAN status to detect changes
previous_lan_status = None

def draw_lan_status(connected, ip_address):
    """Draw LAN connection status on the screen."""
    # Clear the LAN status area
    tft.fillrect((5, 5), (150, 30), ST7735.TFT.BLACK)
    
    # Draw title
    tft.text((5, 5), "LAN Status", ST7735.TFT.WHITE, font5x8.font5x8)
    
    # Draw connection status
    connect_text = f"Connect: {str(connected)}"
    connect_color = ST7735.TFT.GREEN if connected else ST7735.TFT.RED
    tft.text((5, 15), connect_text, connect_color, font5x8.font5x8)
    
    # Draw IP address
    ip_text = f"IP: {ip_address}"
    tft.text((5, 25), ip_text, ST7735.TFT.CYAN, font5x8.font5x8)

# Draw initial status
draw_lan_status(False, "0.0.0.0")

while True:
    # Read all button states
    #previous_states = update_dpad_states(tft, dpad, previous_states, button_positions)
    
    if uart.any():       # Check if data is available
        data = uart.readline()   # Read up to newline
        if data:
            try:
                decoded = data.decode().strip()
                print("Received:", decoded)
                
                # Parse comma-delimited data: connected,ip_address
                parts = decoded.split(',')
                if len(parts) >= 2:
                    # Parse connection status (true/false or 1/0)
                    connected_str = parts[0].strip().lower()
                    connected = connected_str in ['true', '1', 'yes']
                    
                    # Parse IP address
                    ip_address = parts[1].strip()
                    
                    # Create current status tuple
                    current_lan_status = (connected, ip_address)
                    
                    # Only update display if status changed
                    if current_lan_status != previous_lan_status:
                        draw_lan_status(connected, ip_address)
                        previous_lan_status = current_lan_status
            except Exception as e:
                print("Error parsing UART data:", e)
    
    # Small delay
    time.sleep(0.05)
