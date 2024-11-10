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
    