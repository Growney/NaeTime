namespace NaeTime.Hardware.Frequency;
public struct Band
{
    internal Band(byte id, string name, string shortName, IEnumerable<BandFrequency> frequencies)
    {
        Id = id;
        ShortName = shortName;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Frequencies = frequencies ?? throw new ArgumentNullException(nameof(frequencies));
    }

    public static readonly IEnumerable<Band> Bands
    = new List<Band>()
    {
        new (0, "A", "A", new List<BandFrequency>
            {
                new("A1",(int)RaceBandA.A1),
                new("A2",(int)RaceBandA.A2),
                new("A3",(int)RaceBandA.A3),
                new("A4",(int)RaceBandA.A4),
                new("A5",(int)RaceBandA.A5),
                new("A6",(int)RaceBandA.A6),
                new("A7",(int)RaceBandA.A7),
                new("A8",(int)RaceBandA.A8)
            }),
        new (1, "B", "B", new List<BandFrequency>
        {
                new("B1",(int)RaceBandB.B1),
                new("B2",(int)RaceBandB.B2),
                new("B3",(int)RaceBandB.B3),
                new("B4",(int)RaceBandB.B4),
                new("B5",(int)RaceBandB.B5),
                new("B6",(int)RaceBandB.B6),
                new("B7",(int)RaceBandB.B7),
                new("B8",(int)RaceBandB.B8)
            }),
        new (2, "E", "E", new List<BandFrequency>
        {
                new("E1",(int)RaceBandE.E1),
                new("E2",(int)RaceBandE.E2),
                new("E3",(int)RaceBandE.E3),
                new("E4",(int)RaceBandE.E4),
                new("E5",(int)RaceBandE.E5),
                new("E6",(int)RaceBandE.E6),
                new("E7",(int)RaceBandE.E7),
                new("E8",(int)RaceBandE.E8)
            }),
        new (3, "F", "F", new List<BandFrequency>
        {
                new("F1",(int)RaceBandF.F1),
                new("F2",(int)RaceBandF.F2),
                new("F3",(int)RaceBandF.F3),
                new("F4",(int)RaceBandF.F4),
                new("F5",(int)RaceBandF.F5),
                new("F6",(int)RaceBandF.F6),
                new("F7",(int)RaceBandF.F7),
                new("F8",(int)RaceBandF.F8)
            }),
        new (4, "R", "R", new List<BandFrequency>
        {
                new("R1",(int)RaceBandR.R1),
                new("R2",(int)RaceBandR.R2),
                new("R3",(int)RaceBandR.R3),
                new("R4",(int)RaceBandR.R4),
                new("R5",(int)RaceBandR.R5),
                new("R6",(int)RaceBandR.R6),
                new("R7",(int)RaceBandR.R7),
                new("R8",(int)RaceBandR.R8)
            }),
        new (5, "DJI 25Mbps", "DJI 25", new List<BandFrequency>
        {
                new("CH1",(int)DJI25Mbps.CH1),
                new("CH2",(int) DJI25Mbps.CH2),
                new("CH3",(int) DJI25Mbps.CH3),
                new("CH4",(int) DJI25Mbps.CH4),
                new("CH5",(int) DJI25Mbps.CH5),
                new("CH6",(int) DJI25Mbps.CH6),
                new("CH7",(int) DJI25Mbps.CH7),
                new("CH8",(int) DJI25Mbps.CH8)
            }),
        new (6, "DJI 50Mbps","DJI 50", new List<BandFrequency>
        {
                new("CH1",(int)DJI50Mbps.CH1),
                new("CH2",(int) DJI50Mbps.CH2),
                new("CH3",(int) DJI50Mbps.CH3),
                new("CH8",(int) DJI50Mbps.CH8)
            }),
        new (7, "DJI 03","DJI 03", new List<BandFrequency>
        {
                new("CH1",(int)DJI0350Mbps.CH1),
                new("CH2",(int)DJI0350Mbps.CH2),
                new("CH3",(int)DJI0350Mbps.CH3),
            }),
        new (8, "HDZero","HDZ", new List<BandFrequency>
        {
                new("R1",(int)HDZero.R1),
                new("R2",(int)HDZero.R2),
                new("R3",(int)HDZero.R3),
                new("R4",(int)HDZero.R4),
                new("R5",(int)HDZero.R5),
                new("R6",(int)HDZero.R6),
                new("R7",(int)HDZero.R7),
                new("R8",(int)HDZero.R8)
            }),
        new (9, "Walksnail Race","WS Race", new List<BandFrequency>
            {
                new("R1",(int)WalksnailRace.R1),
                new("R2",(int)WalksnailRace.R2),
                new("R3",(int)WalksnailRace.R3),
                new("R4",(int)WalksnailRace.R4),
                new("R5",(int)WalksnailRace.R5),
                new("R6",(int)WalksnailRace.R6),
                new("R7",(int)WalksnailRace.R7),
                new("R8",(int)WalksnailRace.R8)
            }),
        new (10, "Walksnail 25Mbps","WS 25", new List<BandFrequency>
        {
                new("CH1",(int)Walksnail25Mbps.CH1),
                new("CH2",(int)Walksnail25Mbps.CH2),
                new("CH3",(int)Walksnail25Mbps.CH3),
                new("CH4",(int)Walksnail25Mbps.CH4),
                new("CH5",(int)Walksnail25Mbps.CH5),
                new("CH6",(int)Walksnail25Mbps.CH6),
                new("CH7",(int)Walksnail25Mbps.CH7),
                new("CH8",(int)Walksnail25Mbps.CH8)
            }),
        new (11, "Walksnail 50Mbps","WS 50", new List<BandFrequency>
        {
                new("CH1",(int)Walksnail50Mbps.CH1),
                new("CH2",(int)Walksnail50Mbps.CH2),
                new("CH3",(int)Walksnail50Mbps.CH3),
                new("CH8",(int)Walksnail50Mbps.CH8)
            })
    };

    public byte Id { get; }
    public string Name { get; }
    public string ShortName { get; }
    public IEnumerable<BandFrequency> Frequencies { get; }
}
