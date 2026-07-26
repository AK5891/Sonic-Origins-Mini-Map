namespace Sonic_Tracker.Memory;

public sealed class Sonic1Reader
{
    private const double FixedPointScale = 65536.0;
    private static readonly nint SonicXOffset = new(0x3151910);
    private static readonly nint SonicYOffset = new(0x3151914);

    private readonly MemoryReader _memoryReader;

    public Sonic1Reader(MemoryReader memoryReader)
    {
        _memoryReader = memoryReader
            ?? throw new ArgumentNullException(nameof(memoryReader));
    }

    public SonicPosition GetPosition()
    {
        int rawX =
            _memoryReader.ReadInt32AtModuleOffset(
                SonicXOffset);

        int rawY =
            _memoryReader.ReadInt32AtModuleOffset(
                SonicYOffset);

        return new SonicPosition(
            rawX / FixedPointScale,
            rawY / FixedPointScale,
            rawX,
            rawY);
    }
}
