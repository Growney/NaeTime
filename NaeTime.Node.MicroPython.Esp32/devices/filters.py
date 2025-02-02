import math

class MeanFilter:
    def __init__(self, span: int):
        self._values: list[int] = []
        self._span = span
        self._sum = 0
    
    def get_value(self, nextValue: int):
        self._values.append(nextValue)
        self._sum += nextValue
        
        if len(self._values) > self._span:
            self._sum -= self._values.pop(0)
        
        return self._sum // len(self._values)
    
class LowPassFilter:
    def __init__(self, cutoff_frequency, sampling_rate_ms):
        self.cutoff_frequency = cutoff_frequency
        self.sampling_rate_ms = sampling_rate_ms
        self.alpha = self.calculate_alpha(cutoff_frequency, sampling_rate_ms)
        self.previous_value = None

    def calculate_alpha(self, cutoff_frequency, sampling_rate_ms):
        dt = sampling_rate_ms / 1000.0  # Convert milliseconds to seconds
        rc = 1.0 / (2 * math.pi * cutoff_frequency)
        return dt / (rc + dt)

    def get_value(self, next_value):
        if self.previous_value is None:
            self.previous_value = next_value
            return next_value

        filtered_value = self.previous_value + self.alpha * (next_value - self.previous_value)
        self.previous_value = filtered_value
        return int(filtered_value)