# main.py
from machine import Pin, SPI, UART, PWM
import led
import time
import ST7735  # Your driver file
import font5x8  # Optional font module if you have one
from dpad import DPad
from menu import Menu
from machine import PWM
from uart_comms import UARTCommsClient
from frequencies import (BANDS, BAND_NAMES, CUSTOM_BAND_ID,
                         find_band_channel, get_channel_display_values,
                         get_band_id, get_channel_freq)

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
dpad = DPad(up_pin=25, down_pin=27, left_pin=14, right_pin=26)

leds = led.RGBLed(red_pin=22, green_pin=32, blue_pin=33)

buzzer = Pin(13, Pin.OUT)

backlight = PWM(Pin(23), freq=1000, duty_u16=65535)

uartTx = Pin(17)
uartRx = Pin(16, Pin.IN, Pin.PULL_DOWN)

comms_uart = UART(2, baudrate=115200, tx=uartTx, rx=uartRx, timeout=0)
uart_client = UARTCommsClient(comms_uart)

# Create Settings submenu
network_menu = Menu(tft, dpad, title="Network Settings")

# Non-interactive status line at the top of the network menu.
# Updated with a live query each time the user enters the menu.
network_status_item = network_menu.add_item("Net: Unknown")
network_status_item.disabled = True

def on_dhcp_toggle(value):
    print(f"DHCP {'enabled' if value else 'disabled'}")
    # Grey out the static address fields when DHCP is active
    network_menu.set_item_disabled(ip_item, value)
    network_menu.set_item_disabled(subnet_item, value)
    network_menu.set_item_disabled(gateway_item, value)

def on_ip_change(value):
    print(f"IP address set to {value}")

def on_subnet_change(value):
    print(f"Subnet mask set to {value}")

def on_gateway_change(value):
    print(f"Gateway set to {value}")

# --- Populate network menu ---
dhcp_item = network_menu.add_toggle_item("DHCP",
                                          on_change=on_dhcp_toggle,
                                          initial=False)
ip_item = network_menu.add_ip_address_item("IP Address",
                                            ip="0.0.0.0",
                                            on_change=on_ip_change)
subnet_item = network_menu.add_ip_address_item("Subnet",
                                                ip="0.0.0.0",
                                                on_change=on_subnet_change)
gateway_item = network_menu.add_ip_address_item("Gateway",
                                                 ip="0.0.0.0",
                                                 on_change=on_gateway_change)

def apply_network_config():
    """Read current menu values and send a ConfigureNetwork command to the comms board."""
    dhcp    = dhcp_item.value_index == 1
    ip      = ip_item.get_value()
    subnet  = subnet_item.get_value()
    gateway = gateway_item.get_value()
    print(f"Applying network config: dhcp={dhcp} ip={ip} subnet={subnet} gateway={gateway}")
    result = uart_client.configure_network(dhcp, ip, subnet, gateway, timeout_ms=5000)
    if result is True:
        network_status_item.label = "Applied OK"
    elif result is False:
        network_status_item.label = "Apply Failed"
    else:
        network_status_item.label = "Apply Timeout"

network_menu.add_item("Apply", callback=apply_network_config)
network_menu.add_back_item()

# Start with static fields disabled until we know the DHCP state
ip_item.disabled = True
subnet_item.disabled = True
gateway_item.disabled = True

# --- Radio menu ---
radio_menu = Menu(tft, dpad, title="Radio Config")

# Status line updated each time the menu is opened.
radio_status_item = radio_menu.add_item("Status: Unknown")
radio_status_item.disabled = True

# Cached lane configurations retrieved from the comms board.
_lane_configs = []

def _refresh_channel_list(band_list_idx):
    """Replace the channel item's values list for the given band."""
    radio_channel_item.values = get_channel_display_values(band_list_idx)
    radio_channel_item.value_index = 0


def _load_lane_into_editor(lane_idx):
    """Populate band / channel / custom-freq items from the cached lane config."""
    if lane_idx < len(_lane_configs):
        cfg = _lane_configs[lane_idx]
        band_id  = cfg['band_id']
        freq_mhz = cfg['frequency_mhz']
    else:
        band_id  = 0
        freq_mhz = 5800

    bi, ci = find_band_channel(band_id, freq_mhz)
    if bi is not None:
        # Known band and channel — select them by name.
        radio_band_item.value_index = bi
        _refresh_channel_list(bi)
        radio_channel_item.value_index = ci
        radio_menu.set_item_disabled(radio_channel_item, False)
        radio_menu.set_item_disabled(radio_custom_freq_item, True)
    else:
        # Unknown / custom frequency — fall back to the Custom entry.
        radio_band_item.value_index = len(BANDS)  # "Custom" sentinel
        radio_menu.set_item_disabled(radio_channel_item, True)
        radio_menu.set_item_disabled(radio_custom_freq_item, False)
        radio_custom_freq_item._value = freq_mhz

def on_radio_lane_change(value):
    """Called (live) when the user changes the lane selector."""
    _load_lane_into_editor(int(value))
    if radio_menu.visible:
        radio_menu._draw_full_menu()

radio_lane_item = radio_menu.add_numeric_item(
    "Lane", value=0, increment=1, min_value=0, max_value=7,
    on_change=on_radio_lane_change, live_update=True)


def on_radio_band_change(value):
    """Called (live) when the user cycles through bands."""
    if value == "Custom":
        radio_menu.set_item_disabled(radio_channel_item, True)
        radio_menu.set_item_disabled(radio_custom_freq_item, False)
    else:
        bi = BAND_NAMES.index(value)
        _refresh_channel_list(bi)
        radio_menu.set_item_disabled(radio_channel_item, False)
        radio_menu.set_item_disabled(radio_custom_freq_item, True)
    if radio_menu.visible:
        radio_menu._draw_full_menu()


def on_radio_channel_change(value):
    """Called (live) when the user cycles through channels."""
    is_custom = (value == "Custom")
    radio_menu.set_item_disabled(radio_custom_freq_item, not is_custom)
    if radio_menu.visible:
        radio_menu._draw_full_menu()


radio_band_item = radio_menu.add_value_item(
    "Band", values=BAND_NAMES,
    on_change=on_radio_band_change, live_update=True)

radio_channel_item = radio_menu.add_value_item(
    "Channel", values=get_channel_display_values(0),
    on_change=on_radio_channel_change, live_update=True)

radio_custom_freq_item = radio_menu.add_numeric_item(
    "Custom MHz", value=5800, increment=1, min_value=5000, max_value=6000,
    fmt="{} MHz")
radio_custom_freq_item.disabled = True

def apply_radio_config():
    """Read current menu values and send a TuneLane command to the comms board."""
    lane      = radio_lane_item.get_value()
    band_name = radio_band_item.get_value()

    if band_name == "Custom":
        # Fully custom: use CUSTOM_BAND_ID and the manually entered frequency.
        band_id  = CUSTOM_BAND_ID
        freq_mhz = radio_custom_freq_item.get_value()
    else:
        bi       = BAND_NAMES.index(band_name)
        band_id  = get_band_id(bi)
        chan_val = radio_channel_item.get_value()
        if chan_val == "Custom":
            # Known band, custom channel frequency.
            freq_mhz = radio_custom_freq_item.get_value()
        else:
            ci       = radio_channel_item.value_index
            freq_mhz = get_channel_freq(bi, ci)

    print(f"Applying radio config: lane={lane} band_id={band_id} freq={freq_mhz}")
    result = uart_client.tune_lane(lane, band_id, freq_mhz, timeout_ms=5000)
    if result is True:
        radio_status_item.label = "Applied OK"
    elif result is False:
        radio_status_item.label = "Apply Failed"
    else:
        radio_status_item.label = "Apply Timeout"
    if radio_menu.visible:
        radio_menu._draw_item(0, False)  # refresh status row

radio_menu.add_item("Apply", callback=apply_radio_config)
radio_menu.add_back_item()

def open_radio_menu():
    """Query the comms board for lane configurations, populate the menu, then open it."""
    global _lane_configs
    result = uart_client.query_lane_configurations(lane_mask=0xFF, timeout_ms=5000)
    print(f"Queried lane configurations: {result}")
    if result is not None:
        _lane_configs = result['lanes']
        radio_status_item.label = "Lanes OK ({})".format(len(_lane_configs))
        # Initialise display with lane 0 values.
        radio_lane_item._value = 0
        _load_lane_into_editor(0)
    else:
        _lane_configs = []
        radio_status_item.label = "Status: Error"
        radio_lane_item._value = 0
        radio_band_item.value_index = 0
        _refresh_channel_list(0)
        radio_custom_freq_item._value = 5800
    menu.open_submenu(radio_menu)

# --- Tests menu ---
tests_menu = Menu(tft, dpad, title="Tests")

test_buzzer_on = False
test_led_on = False
test_led_hue = 0

def on_test_buzzer_toggle(value):
    global test_buzzer_on
    test_buzzer_on = value
    buzzer.value(1 if value else 0)

def on_test_led_toggle(value):
    global test_led_on
    test_led_on = value
    if not value:
        leds.off()
    elif value:
        r, g, b = wheel(test_led_hue)
        leds.set_colour(r, g, b)

def on_test_led_hue_change(value):
    global test_led_hue
    test_led_hue = value
    r, g, b = wheel(test_led_hue)
    leds.set_colour(r, g, b)

tests_menu.add_toggle_item("Buzzer",
                            on_change=on_test_buzzer_toggle,
                            initial=False,
                            live_update=True)
tests_menu.add_separator()
tests_menu.add_toggle_item("LED",
                            on_change=on_test_led_toggle,
                            initial=False)
tests_menu.add_numeric_item("LED Hue",
                             value=0,
                             increment=1,
                             min_value=0,
                             max_value=255,
                             on_change=on_test_led_hue_change,
                             fmt="{}",
                             live_update=True)
tests_menu.add_back_item()

# Create and configure main menu
menu = Menu(tft, dpad, title="NaeTime Menu")

def open_network_menu():
    """Query the comms board for full network status, populate all menu items,
    then open the Network Settings submenu."""
    result = uart_client.query_network_status(timeout_ms=5000)
    print(f"Queried network status: {result}")
    if result is not None:
        connected = result['connected']
        dhcp      = result['dhcp']
        ip        = result['ip']
        subnet    = result['subnet']
        gateway   = result['gateway']

        network_status_item.label = "Net: Connected" if connected else "Net: Disconnected"

        # Update DHCP toggle (value_index 0 = Off/False, 1 = On/True)
        dhcp_item.value_index = 1 if dhcp else 0

        # Update IP address fields directly from the comms board values
        ip_item._segments      = [int(x) for x in ip.split('.')]
        subnet_item._segments  = [int(x) for x in subnet.split('.')]
        gateway_item._segments = [int(x) for x in gateway.split('.')]

        # Enable or grey out static fields based on DHCP state
        network_menu.set_item_disabled(ip_item, dhcp)
        network_menu.set_item_disabled(subnet_item, dhcp)
        network_menu.set_item_disabled(gateway_item, dhcp)
    else:
        network_status_item.label = "Net: Unknown"

    menu.open_submenu(network_menu)

menu.add_item("Network", callback=open_network_menu)
menu.add_item("Radio", callback=open_radio_menu)
menu.add_submenu("Tests", tests_menu)

# Show the menu
menu.show()

print("NaeTime Menu System Started!")

def wheel(pos):
    """Map a 0-255 position to an RGB rainbow colour."""
    pos = pos & 255
    if pos < 85:
        return (255 - pos * 3, pos * 3, 0)
    elif pos < 170:
        pos -= 85
        return (0, 255 - pos * 3, pos * 3)
    else:
        pos -= 170
        return (pos * 3, 0, 255 - pos * 3)

buzzer.value(1)
time.sleep(0.1)
buzzer.value(0)

# Main loop
hue = 0
while True:
    menu.update()


# async def sound_buzzer(pattern):
#     global buzzer_pin
#     for step in pattern:

#         buzzer_pin.value(step[0])
#         await asyncio.sleep_ms(step[1])
    
#     buzzer_pin.value(0)