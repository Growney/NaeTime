namespace RX5808.Enumeration;

[Flags]
public enum PowerDownControlRegister
{
    PD_PLL1D8 = 0x0001,
    PD_DIV80 = 0x0002,
    PD_MIXER = 0x0004,
    PD_IFABF = 0x0008,
    PD_REG1D8 = 0x0010,
    PD_6M5 = 0x0020,
    PD_AU6M5 = 0x0040,
    PD_6M = 0x0080,
    PD_AU6M = 0x0100,
    PD_SYN = 0x0200,
    PD_5GVCO = 0x0400,
    PD_DIV4 = 0x0800,
    PD_BC = 0x1000,
    PD_REGIF = 0x2000,
    PD_REGBS = 0x4000,
    PD_RSSI_SQUELCH = 0x8000,
    PD_IFAF = 0x10000,
    PD_IF_DEMOD = 0x20000,
    PD_VAMP = 0x40000,
    PD_VCLAMP = 0x80000,

    All = 0xFFFFF,
    None = 0x00000

};