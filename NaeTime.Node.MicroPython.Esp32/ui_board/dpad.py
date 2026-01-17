from machine import Pin
import time

class _ButtonState:
    """Tracks the raw hardware pin, logical state, transitions, and timing."""
    def __init__(self, pin: Pin):
        self.pin = pin
        self.current = False       # True = pressed
        self.previous = False
        self.state = "unpressed"   # unpressed, button_down, pressed, button_up
        self.held_time = 0         # ms
        self._pressed_timestamp = 0

    def update(self):
        # Active-low button logic: pressed = 0
        raw = (self.pin.value() == 0)
        now = time.ticks_ms()

        self.previous = self.current
        self.current = raw

        # Detect transitions
        if not self.previous and self.current:
            # Just pressed
            self.state = "button_down"
            self._pressed_timestamp = now
            self.held_time = 0

        elif self.previous and self.current:
            # Being held
            self.state = "pressed"
            self.held_time = time.ticks_diff(now, self._pressed_timestamp)

        elif self.previous and not self.current:
            # Just released
            self.state = "button_up"
            self.held_time = time.ticks_diff(now, self._pressed_timestamp)

        else:
            # Idle
            self.state = "unpressed"
            self.held_time = 0

        return self.state


class DPad:
    def __init__(self, up_pin, down_pin, left_pin, right_pin, pull=Pin.PULL_UP):
        """
        Initialize a D-pad with four directional buttons.
        Tracks states: unpressed, button_down, pressed, button_up
        Also tracks held duration (ms).
        """
        self.buttons = {
            "up":    _ButtonState(Pin(up_pin, Pin.IN, pull)),
            "down":  _ButtonState(Pin(down_pin, Pin.IN, pull)),
            "left":  _ButtonState(Pin(left_pin, Pin.IN, pull)),
            "right": _ButtonState(Pin(right_pin, Pin.IN, pull))
        }

    def read(self):
        """
        Update and return state of all buttons:
        {
            "up":   {"state": "...", "held_ms": N},
            ...
        }
        """
        results = {}
        for name, btn in self.buttons.items():
            btn.update()
            results[name] = {
                "state": btn.state,
                "held_ms": btn.held_time
            }
        return results
