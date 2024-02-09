
namespace NaeTime.Timing.Frequency;
public struct Band
{
    internal Band(byte id, string name, IEnumerable<BandFrequency> frequencies)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Frequencies = frequencies ?? throw new ArgumentNullException(nameof(frequencies));
    }

    public static readonly IEnumerable<Band> Bands
    = new List<Band>()
    {
        new (0, "A", new List<BandFrequency>
            {
                new BandFrequency("A1",(int)RaceBandA.A1),
                new BandFrequency("A2",(int)RaceBandA.A2),
                new BandFrequency("A3",(int)RaceBandA.A3),
                new BandFrequency("A4",(int)RaceBandA.A4),
                new BandFrequency("A5",(int)RaceBandA.A5),
                new BandFrequency("A6",(int)RaceBandA.A6),
                new BandFrequency("A7",(int)RaceBandA.A7),
                new BandFrequency("A8",(int)RaceBandA.A8)
            }),
        new (1, "B", new List<BandFrequency>
        {
                new BandFrequency("B1",(int)RaceBandB.B1),
                new BandFrequency("B2",(int)RaceBandB.B2),
                new BandFrequency("B3",(int)RaceBandB.B3),
                new BandFrequency("B4",(int)RaceBandB.B4),
                new BandFrequency("B5",(int)RaceBandB.B5),
                new BandFrequency("B6",(int)RaceBandB.B6),
                new BandFrequency("B7",(int)RaceBandB.B7),
                new BandFrequency("B8",(int)RaceBandB.B8)
            }),
        new (2, "E", new List<BandFrequency>
        {
                new BandFrequency("E1",(int)RaceBandE.E1),
                new BandFrequency("E2",(int)RaceBandE.E2),
                new BandFrequency("E3",(int)RaceBandE.E3),
                new BandFrequency("E4",(int)RaceBandE.E4),
                new BandFrequency("E5",(int)RaceBandE.E5),
                new BandFrequency("E6",(int)RaceBandE.E6),
                new BandFrequency("E7",(int)RaceBandE.E7),
                new BandFrequency("E8",(int)RaceBandE.E8)
            }),
        new (3, "F", new List<BandFrequency>
        {
                new BandFrequency("F1",(int)RaceBandF.F1),
                new BandFrequency("F2",(int)RaceBandF.F2),
                new BandFrequency("F3",(int)RaceBandF.F3),
                new BandFrequency("F4",(int)RaceBandF.F4),
                new BandFrequency("F5",(int)RaceBandF.F5),
                new BandFrequency("F6",(int)RaceBandF.F6),
                new BandFrequency("F7",(int)RaceBandF.F7),
                new BandFrequency("F8",(int)RaceBandF.F8)
            }),
        new (4, "R", new List<BandFrequency>
        {
                new BandFrequency("R1",(int)RaceBandR.R1),
                new BandFrequency("R2",(int)RaceBandR.R2),
                new BandFrequency("R3",(int)RaceBandR.R3),
                new BandFrequency("R4",(int)RaceBandR.R4),
                new BandFrequency("R5",(int)RaceBandR.R5),
                new BandFrequency("R6",(int)RaceBandR.R6),
                new BandFrequency("R7",(int)RaceBandR.R7),
                new BandFrequency("R8",(int)RaceBandR.R8)
            }),
        new (5, "DJI 25Mbps", new List<BandFrequency>
        {
                new BandFrequency("CH1",(int)DJI25Mbps.CH1),
                new BandFrequency("CH2",(int) DJI25Mbps.CH2),
                new BandFrequency("CH3",(int) DJI25Mbps.CH3),
                new BandFrequency("CH4",(int) DJI25Mbps.CH4),
                new BandFrequency("CH5",(int) DJI25Mbps.CH5),
                new BandFrequency("CH6",(int) DJI25Mbps.CH6),
                new BandFrequency("CH7",(int) DJI25Mbps.CH7),
                new BandFrequency("CH8",(int) DJI25Mbps.CH8)
            }),
        new (6, "DJI 50Mbps", new List<BandFrequency>
        {
                new BandFrequency("CH1",(int)DJI50Mbps.CH1),
                new BandFrequency("CH2",(int) DJI50Mbps.CH2),
                new BandFrequency("CH3",(int) DJI50Mbps.CH3),
                new BandFrequency("CH8",(int) DJI50Mbps.CH8)
            }),
        new (7, "DJI 03", new List<BandFrequency>
        {
                new BandFrequency("CH1",(int)DJI0350Mbps.CH1),
                new BandFrequency("CH2",(int)DJI0350Mbps.CH2),
                new BandFrequency("CH3",(int)DJI0350Mbps.CH3),
            }),
        new (8, "HDZero", new List<BandFrequency>
        {
                new BandFrequency("R1",(int)HDZero.R1),
                new BandFrequency("R2",(int)HDZero.R2),
                new BandFrequency("R3",(int)HDZero.R3),
                new BandFrequency("R4",(int)HDZero.R4),
                new BandFrequency("R5",(int)HDZero.R5),
                new BandFrequency("R6",(int)HDZero.R6),
                new BandFrequency("R7",(int)HDZero.R7),
                new BandFrequency("R8",(int)HDZero.R8)
            }),
        new (9, "Walksnail Race", new List<BandFrequency>
            {
                new BandFrequency("R1",(int)WalksnailRace.R1),
                new BandFrequency("R2",(int)WalksnailRace.R2),
                new BandFrequency("R3",(int)WalksnailRace.R3),
                new BandFrequency("R4",(int)WalksnailRace.R4),
                new BandFrequency("R5",(int)WalksnailRace.R5),
                new BandFrequency("R6",(int)WalksnailRace.R6),
                new BandFrequency("R7",(int)WalksnailRace.R7),
                new BandFrequency("R8",(int)WalksnailRace.R8)
            }),
        new (10, "Walksnail 25Mbps", new List<BandFrequency>
        {
                new BandFrequency("CH1",(int)Walksnail25Mbps.CH1),
                new BandFrequency("CH2",(int)Walksnail25Mbps.CH2),
                new BandFrequency("CH3",(int)Walksnail25Mbps.CH3),
                new BandFrequency("CH4",(int)Walksnail25Mbps.CH4),
                new BandFrequency("CH5",(int)Walksnail25Mbps.CH5),
                new BandFrequency("CH6",(int)Walksnail25Mbps.CH6),
                new BandFrequency("CH7",(int)Walksnail25Mbps.CH7),
                new BandFrequency("CH8",(int)Walksnail25Mbps.CH8)
            }),
        new (11, "Walksnail 50Mbps", new List<BandFrequency>
        {
                new BandFrequency("CH1",(int)Walksnail50Mbps.CH1),
                new BandFrequency("CH2",(int)Walksnail50Mbps.CH2),
                new BandFrequency("CH3",(int)Walksnail50Mbps.CH3),
                new BandFrequency("CH8",(int)Walksnail50Mbps.CH8)
            })
    };

    public byte Id { get; }
    public string Name { get; }
    public IEnumerable<BandFrequency> Frequencies { get; }
}
