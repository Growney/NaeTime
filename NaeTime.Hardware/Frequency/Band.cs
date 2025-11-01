namespace NaeTime.Hardware.Frequency;
public struct Band
{
    public static readonly Band A = new(0, "A", "A", new List<BandFrequency>
            {
                new(0,"A1",(int)RaceBandA.A1),
                new(0,"A2",(int)RaceBandA.A2),
                new(0,"A3",(int)RaceBandA.A3),
                new(0,"A4",(int)RaceBandA.A4),
                new(0,"A5",(int)RaceBandA.A5),
                new(0,"A6",(int)RaceBandA.A6),
                new(0,"A7",(int)RaceBandA.A7),
                new(0,"A8",(int)RaceBandA.A8)
            });
    public static readonly Band B = new(1, "B", "B", new List<BandFrequency>
            {
                new(1,"B1",(int)RaceBandB.B1),
                new(1,"B2",(int)RaceBandB.B2),
                new(1,"B3",(int)RaceBandB.B3),
                new(1,"B4",(int)RaceBandB.B4),
                new(1,"B5",(int)RaceBandB.B5),
                new(1,"B6",(int)RaceBandB.B6),
                new(1,"B7",(int)RaceBandB.B7),
                new(1,"B8",(int)RaceBandB.B8)
            });
    public static readonly Band E = new(2, "E", "E", new List<BandFrequency>
            {
                new(2,"E1",(int)RaceBandE.E1),
                new(2,"E2",(int)RaceBandE.E2),
                new(2,"E3",(int)RaceBandE.E3),
                new(2,"E4",(int)RaceBandE.E4),
                new(2,"E5",(int)RaceBandE.E5),
                new(2,"E6",(int)RaceBandE.E6),
                new(2,"E7",(int)RaceBandE.E7),
                new(2,"E8",(int)RaceBandE.E8)
            });

    public static readonly Band F = new(3, "F", "F", new List<BandFrequency>
            {
                new(3,"F1",(int)RaceBandF.F1),
                new(3,"F2",(int)RaceBandF.F2),
                new(3,"F3",(int)RaceBandF.F3),
                new(3,"F4",(int)RaceBandF.F4),
                new(3,"F5",(int)RaceBandF.F5),
                new(3,"F6",(int)RaceBandF.F6),
                new(3,"F7",(int)RaceBandF.F7),
                new(3,"F8",(int)RaceBandF.F8)
            });

    public static readonly Band R = new(4, "R", "R", new List<BandFrequency>
            {
                new(4,"R1",(int)RaceBandR.R1),
                new(4,"R2",(int)RaceBandR.R2),
                new(4,"R3",(int)RaceBandR.R3),
                new(4,"R4",(int)RaceBandR.R4),
                new(4,"R5",(int)RaceBandR.R5),
                new(4,"R6",(int)RaceBandR.R6),
                new(4,"R7",(int)RaceBandR.R7),
                new(4,"R8",(int)RaceBandR.R8)
            });

    public static readonly Band DJI25 = new(5, "DJI 25Mbps", "DJI 25", new List<BandFrequency>
            {
                new(5,"CH1",(int)DJI25Mbps.CH1),
                new(5,"CH2",(int) DJI25Mbps.CH2),
                new(5,"CH3",(int) DJI25Mbps.CH3),
                new(5,"CH4",(int) DJI25Mbps.CH4),
                new(5,"CH5",(int) DJI25Mbps.CH5),
                new(5,"CH6",(int) DJI25Mbps.CH6),
                new(5,"CH7",(int) DJI25Mbps.CH7),
                new(5,"CH8",(int) DJI25Mbps.CH8)
            });

    public static readonly Band DJI50 = new(6, "DJI 50Mbps", "DJI 50", new List<BandFrequency>
            {
                new(6,"CH1",(int)DJI50Mbps.CH1),
                new(6,"CH2",(int) DJI50Mbps.CH2),
                new(6,"CH3",(int) DJI50Mbps.CH3),
                new(6,"CH8",(int) DJI50Mbps.CH8)
            });

    public static readonly Band DJI03 = new(7, "DJI 03", "DJI 03", new List<BandFrequency>
            {
                new(7,"CH1",(int)DJI0350Mbps.CH1),
                new(7,"CH2",(int)DJI0350Mbps.CH2),
                new(7,"CH3",(int)DJI0350Mbps.CH3)
            });

    public static readonly Band HDZero = new(8, "HDZero", "HDZ", new List<BandFrequency>
            {
                new(8,"R1",(int)Frequency.HDZero.R1),
                new(8,"R2",(int)Frequency.HDZero.R2),
                new(8,"R3",(int)Frequency.HDZero.R3),
                new(8,"R4",(int)Frequency.HDZero.R4),
                new(8,"R5",(int)Frequency.HDZero.R5),
                new(8,"R6",(int)Frequency.HDZero.R6),
                new(8,"R7",(int)Frequency.HDZero.R7),
                new(8,"R8",(int)Frequency.HDZero.R8)
            });
    public static readonly Band WalksnailRace = new(9, "Walksnail Race", "WS Race", new List<BandFrequency>
            {
                new(9,"R1",(int)Frequency.HDZero.R1),
                new(9,"R2",(int)Frequency.HDZero.R2),
                new(9,"R3",(int)Frequency.HDZero.R3),
                new(9,"R4",(int)Frequency.HDZero.R4),
                new(9,"R5",(int)Frequency.HDZero.R5),
                new(9,"R6",(int)Frequency.HDZero.R6),
                new(9,"R7",(int)Frequency.HDZero.R7),
                new(9,"R8",(int)Frequency.HDZero.R8)
            });
    public static readonly Band Walksnail25Mbps = new(10, "Walksnail 25Mbps", "WS 25", new List<BandFrequency>
        {
                new(10,"CH1",(int)Frequency.Walksnail25Mbps.CH1),
                new(10,"CH2",(int)Frequency.Walksnail25Mbps.CH2),
                new(10,"CH3",(int)Frequency.Walksnail25Mbps.CH3),
                new(10,"CH4",(int)Frequency.Walksnail25Mbps.CH4),
                new(10,"CH5",(int)Frequency.Walksnail25Mbps.CH5),
                new(10,"CH6",(int)Frequency.Walksnail25Mbps.CH6),
                new(10,"CH7",(int)Frequency.Walksnail25Mbps.CH7),
                new(10,"CH8",(int)Frequency.Walksnail25Mbps.CH8)
            });

    public static readonly Band Walksnail50Mbps = new(11, "Walksnail 50Mbps", "WS 50", new List<BandFrequency>
        {
                new(11,"CH1",(int)Frequency.Walksnail50Mbps.CH1),
                new(11,"CH2",(int)Frequency.Walksnail50Mbps.CH2),
                new(11,"CH3",(int)Frequency.Walksnail50Mbps.CH3),
                new(11,"CH8",(int)Frequency.Walksnail50Mbps.CH8)
            });
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
        A, B, E, F, R, DJI25, DJI50, DJI03, HDZero,WalksnailRace, Walksnail25Mbps, Walksnail50Mbps
    };

    public byte Id { get; }
    public string Name { get; }
    public string ShortName { get; }
    public IEnumerable<BandFrequency> Frequencies { get; }

    public static bool TryFindById(byte id, int frequencyInMHz, out BandFrequency band)
    {
        band = new();
        Band? bandWithId = Bands.FirstOrDefault(b => b.Id == id);
        if (!bandWithId.HasValue)
        {
            return false;
        }

        BandFrequency? frequency = bandWithId.Value.Frequencies.FirstOrDefault(f => f.FrequencyInMhz == frequencyInMHz);
        if (!frequency.HasValue)
        {
            return false;
        }
        band = frequency.Value;
        return true;
    }
    public static bool TryFindByName(string name, out BandFrequency band)
    {
        band = new();
        BandFrequency? bandWithName = Bands.SelectMany(x => x.Frequencies).FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (!bandWithName.HasValue)
        {
            return false;
        }
        band = bandWithName.Value;
        return true;
    }
}
