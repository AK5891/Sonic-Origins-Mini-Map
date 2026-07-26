namespace Sonic_Tracker.Memory;

public sealed class Sonic2Reader
{
    private const double FixedPointScale = 65536.0;

    private static readonly nint SonicXOffset =
        new(0x3151910);

    private static readonly nint SonicYOffset =
        new(0x3151914);

    private readonly MemoryReader _memoryReader;

    public Sonic2Reader(MemoryReader memoryReader)
    {
        _memoryReader = memoryReader
            ?? throw new ArgumentNullException(nameof(memoryReader));
    }

    public int GetRawSonicX()
    {
        return _memoryReader.ReadInt32AtModuleOffset(
            SonicXOffset);
    }

    public int GetRawSonicY()
    {
        return _memoryReader.ReadInt32AtModuleOffset(
            SonicYOffset);
    }

    public double GetSonicX()
    {
        int rawX = GetRawSonicX();
        return rawX / FixedPointScale;
    }

    public double GetSonicY()
    {
        int rawY = GetRawSonicY();
        return rawY / FixedPointScale;
    }

    public SonicPosition GetPosition()
    {
        int rawX = GetRawSonicX();
        int rawY = GetRawSonicY();

        double x = rawX / FixedPointScale;
        double y = rawY / FixedPointScale;

        return new SonicPosition(
            X: x,
            Y: y,
            RawX: rawX,
            RawY: rawY);
    }
}
