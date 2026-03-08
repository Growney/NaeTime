from machine import Pin, PWM, Timer
import time


class RGBLed:
    def __init__(self, red_pin, green_pin, blue_pin, freq=1000):
        """
        Control an RGB LED using PWM, with optional colour-sequence patterns.

        :param red_pin:   GPIO pin number for the red channel.
        :param green_pin: GPIO pin number for the green channel.
        :param blue_pin:  GPIO pin number for the blue channel.
        :param freq:      PWM frequency in Hz (default 1000).
        """
        self._red   = PWM(Pin(red_pin),   freq=freq, duty_u16=0)
        self._green = PWM(Pin(green_pin), freq=freq, duty_u16=0)
        self._blue  = PWM(Pin(blue_pin),  freq=freq, duty_u16=0)
        self._base_colour = (0, 0, 0)  # restored after a pattern finishes
        self._pattern = []
        self._current_step = 0
        self._timer = Timer(0)

    @staticmethod
    def _byte_to_duty(value: int) -> int:
        """Map a byte (0-255) to a 16-bit duty cycle (0-65535)."""
        return min(int(value), 255) * 257

    def _apply_colour(self, r: int, g: int, b: int):
        """Write colour to PWM channels without updating the base colour."""
        self._red.duty_u16(self._byte_to_duty(r))
        self._green.duty_u16(self._byte_to_duty(g))
        self._blue.duty_u16(self._byte_to_duty(b))

    def set_colour(self, r: int, g: int, b: int):
        """
        Set the LED colour immediately and remember it as the base colour.
        The base colour is restored automatically after a pattern finishes.

        :param r: Red brightness   (0-255).
        :param g: Green brightness (0-255).
        :param b: Blue brightness  (0-255).
        """
        self._timer.deinit()          # cancel any running pattern
        self._base_colour = (r, g, b)
        self._apply_colour(r, g, b)

    def set_pattern(self, pattern):
        """
        Run a one-shot colour-sequence pattern, then restore the base colour.

        Pattern is a list of tuples: ((r, g, b), duration_in_seconds).
        Use (0, 0, 0) as the colour for an 'off' step.
        The pattern runs once and the LED then returns to the colour set by
        the last call to set_colour(), or off if set_colour() was never called.

        Calling set_pattern() immediately starts the pattern — no need to call
        start() separately.

        Example:
            rgb.set_colour(0, 255, 0)          # base: green
            rgb.set_pattern([
                ((255, 0, 0), 0.5),            # red for 0.5 s
                ((  0, 0, 0), 0.25),           # off for 0.25 s
            ])
            # LED returns to green when the pattern ends
        """
        if not pattern:
            return
        self._timer.deinit()
        self._pattern = pattern
        self._current_step = 0
        self._run_pattern()

    def stop(self):
        """Cancel a running pattern and restore the base colour."""
        self._timer.deinit()
        self._apply_colour(*self._base_colour)

    def _run_pattern(self):
        colour, duration = self._pattern[self._current_step]
        self._apply_colour(*colour)
        self._timer.init(
            period=int(duration * 1000),
            mode=Timer.ONE_SHOT,
            callback=self._next_step,
        )

    def _next_step(self, t):
        self._current_step += 1
        if self._current_step < len(self._pattern):
            self._run_pattern()
        else:
            # Pattern finished — restore the base colour
            self._apply_colour(*self._base_colour)

    def off(self):
        """Set the base colour to off and turn the LED off."""
        self.set_colour(0, 0, 0)

    def deinit(self):
        """Stop the pattern and release PWM resources."""
        self._timer.deinit()
        self._red.deinit()
        self._green.deinit()
        self._blue.deinit()


# Example usage:
# rgb = RGBLed(red_pin=25, green_pin=26, blue_pin=27)
#
# Immediate colour (remembered as base):
# rgb.set_colour(0, 255, 0)
#
# Run a pattern then return to base colour (green):
# rgb.set_pattern([
#     ((255, 0, 0), 0.5),   # red for 0.5 s
#     ((  0, 0, 0), 0.25),  # off for 0.25 s
# ])
#
# Cancel mid-pattern and restore base:
# rgb.stop()

class MultiSequenceLED:
    def __init__(self, *pin_numbers):
        self.leds = [Pin(pin, Pin.OUT) for pin in pin_numbers]
        self.patterns = []
        self.current_step = 0
        self.timer = Timer(0)
        self.start_time = time.ticks_ms()

    def set_pattern(self, pattern):
        """
        Set the flashing pattern for the LEDs.
        Pattern should be a list of tuples where each tuple contains
        (states, duration_in_seconds).
        States should be a list of states for each LED.
        Example: [([1, 0], 0.5), ([0, 1], 0.5)] will turn the first LED on and the second off for 0.5 seconds,
        then turn the first LED off and the second on for 0.5 seconds.
        """
        self.patterns = pattern

    def start(self):
        """
        Start the LEDs flashing according to the set pattern.
        """
        if not self.patterns:
            return

        self.current_step = 0
        self.start_time = time.ticks_ms()
        self._run_pattern()

    def _run_pattern(self):
        states, duration = self.patterns[self.current_step]
        for led, state in zip(self.leds, states):
            led.value(state)
        self.timer.init(period=int(duration * 1000), mode=Timer.ONE_SHOT, callback=self._next_step)

    def _next_step(self, t):
        self.current_step = (self.current_step + 1) % len(self.patterns)
        self._run_pattern()

class SequenceLED:
    def __init__(self, pin_number):
        self.led = Pin(pin_number, Pin.OUT)
        self.pattern = []
        self.current_step = 0
        self.timer = Timer(0)
        self.start_time = time.ticks_ms()

    def set_pattern(self, pattern):
        """
        Set the flashing pattern.
        Pattern should be a list of tuples where each tuple contains
        (state, duration_in_seconds).
        Example: [(1, 0.5), (0, 0.5)] will turn the LED on for 0.5 seconds and off for 0.5 seconds.
        """
        self.pattern = pattern

    def start(self):
        """
        Start the LED flashing according to the set pattern.
        """
        if not self.pattern:
            return

        self.current_step = 0
        self.start_time = time.ticks_ms()
        self._run_pattern()

    def _run_pattern(self):
        state, duration = self.pattern[self.current_step]
        self.led.value(state)
        self.timer.init(period=int(duration * 1000), mode=Timer.ONE_SHOT, callback=self._next_step)

    def _next_step(self, t):
        self.current_step = (self.current_step + 1) % len(self.pattern)
        self._run_pattern()

# Example usage:
# led = SequenceLED(2)
# led.set_pattern([(1, 0.5), (0, 0.5)])
# led.start()