# frequencies.py
# Band and frequency data ported from NaeTime.Hardware C# enums/classes.
# Each entry: (band_id, short_name, [(channel_name, freq_mhz), ...])

BANDS = [
    (0,  "A",        [("A1", 5725), ("A2", 5724), ("A3", 5765), ("A4", 5785),
                      ("A5", 5805), ("A6", 5825), ("A7", 5845), ("A8", 5865)]),
    (1,  "B",        [("B1", 5733), ("B2", 5752), ("B3", 5771), ("B4", 5790),
                      ("B5", 5809), ("B6", 5828), ("B7", 5847), ("B8", 5866)]),
    (2,  "E",        [("E1", 5645), ("E2", 5665), ("E3", 5685), ("E4", 5705),
                      ("E5", 5885), ("E6", 5905), ("E7", 5925), ("E8", 5945)]),
    (3,  "F",        [("F1", 5740), ("F2", 5760), ("F3", 5780), ("F4", 5800),
                      ("F5", 5820), ("F6", 5840), ("F7", 5860), ("F8", 5880)]),
    (4,  "R",        [("R1", 5658), ("R2", 5695), ("R3", 5732), ("R4", 5769),
                      ("R5", 5806), ("R6", 5843), ("R7", 5880), ("R8", 5917)]),
    (5,  "DJI 25",   [("CH1", 5660), ("CH2", 5695), ("CH3", 5735), ("CH4", 5770),
                      ("CH5", 5805), ("CH6", 5878), ("CH7", 5914), ("CH8", 5839)]),
    (6,  "DJI 50",   [("CH1", 5695), ("CH2", 5770), ("CH3", 5878), ("CH8", 5839)]),
    (7,  "DJI 03",   [("CH1", 5677), ("CH2", 5794), ("CH3", 5902)]),
    (8,  "HDZ",      [("R1", 5658), ("R2", 5695), ("R3", 5732), ("F2", 5760),
                      ("R4", 5769), ("F4", 5800), ("R5", 5806), ("R6", 5843),
                      ("R7", 5880), ("R8", 5917)]),
    (9,  "WS Race",  [("R1", 5658), ("R2", 5659), ("R3", 5732), ("R4", 5769),
                      ("R5", 5806), ("R6", 5843), ("R7", 5880), ("R8", 5917)]),
    (10, "WS 25",    [("CH1", 5660), ("CH2", 5695), ("CH3", 5735), ("CH4", 5770),
                      ("CH5", 5805), ("CH6", 5878), ("CH7", 5914), ("CH8", 5839)]),
    (11, "WS 50",    [("CH1", 5695), ("CH2", 5770), ("CH3", 5878), ("CH8", 5839)]),
]

# Display names for the band selector, with "Custom" appended.
BAND_NAMES = [b[1] for b in BANDS] + ["Custom"]

# Sentinel band_id used when no standard band matches.
CUSTOM_BAND_ID = 255


def find_band_channel(band_id, freq_mhz):
    """
    Find the BANDS list index and channel index for a given band_id + frequency.

    Returns:
        (band_list_index, channel_index) if found, or (None, None) if not found.
    """
    for bi, (bid, _bname, channels) in enumerate(BANDS):
        if bid == band_id:
            for ci, (_, cfreq) in enumerate(channels):
                if cfreq == freq_mhz:
                    return bi, ci
    return None, None


def get_channel_display_values(band_list_index):
    """
    Return a list of display strings for all channels in the given band,
    formatted as "NAME FREQ" (e.g. "R4 5769"), plus a "Custom" sentinel.
    """
    _, _, channels = BANDS[band_list_index]
    return ["{} {}".format(name, freq) for name, freq in channels] + ["Custom"]


def get_band_id(band_list_index):
    """Return the integer band_id for the given BANDS list index."""
    return BANDS[band_list_index][0]


def get_channel_freq(band_list_index, channel_index):
    """Return the frequency in MHz for the given band and channel index."""
    _, _, channels = BANDS[band_list_index]
    return channels[channel_index][1]
