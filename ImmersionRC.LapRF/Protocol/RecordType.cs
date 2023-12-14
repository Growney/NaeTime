﻿public enum RecordType : ushort
{
    LAPRF_TOR_ERROR = 0xFFFF,

    LAPRF_TOR_RSSI = 0xda01,
    LAPRF_TOR_RFSETUP = 0xda02,
    LAPRF_TOR_STATE_CONTROL = 0xda04,
    LAPRF_TOR_PASSING = 0xda09,
    LAPRF_TOR_SETTINGS = 0xda07,
    LAPRF_TOR_STATUS = 0xda0a,
    LAPRF_TOR_TIME = 0xDA0C,
};