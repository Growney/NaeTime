from machine import Pin, Timer
import time

class MultiSequenceLED:
    def __init__(self, *pin_numbers):
        self.leds = [Pin(pin, Pin.OUT) for pin in pin_numbers]
        self.patterns = []
        self.current_step = 0
        self.timer = Timer(-1)
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
        self.timer = Timer(-1)
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