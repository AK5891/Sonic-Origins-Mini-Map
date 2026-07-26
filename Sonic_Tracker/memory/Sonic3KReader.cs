namespace Sonic_Tracker.Memory;

public sealed class Sonic3KReader
{
    private const double FixedPointScale = 65536.0;

    private static readonly nint SonicXOffset =
        new(0x377E368);

    private static readonly nint SonicYOffset =
        new(0x379DBCC);

    private readonly MemoryReader _memoryReader;

    public Sonic3KReader(
        MemoryReader memoryReader)
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
