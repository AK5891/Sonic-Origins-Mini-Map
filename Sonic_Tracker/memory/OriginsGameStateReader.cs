using Sonic_Tracker.Games;

namespace Sonic_Tracker.Memory;

public sealed class OriginsGameStateReader
{
    private const string ActiveGamePointerModule =
        "steamclient64.dll";

    private static readonly nint ActiveGamePointerModuleOffset =
        new(0x17DE728);

    private static readonly nint ActiveGamePointerOffset =
        nint.Zero;

    private readonly MemoryReader _memoryReader;
    private readonly nint _activeGamePointerAddress;

    public OriginsGameStateReader(MemoryReader memoryReader)
    {
        _memoryReader = memoryReader
            ?? throw new ArgumentNullException(nameof(memoryReader));

        _activeGamePointerAddress =
            _memoryReader.ResolveModuleOffset(
                ActiveGamePointerModule,
                ActiveGamePointerModuleOffset);
    }

    public int GetRawValue()
    {
        nint activeGameAddress =
            _memoryReader.ReadPointer(
                _activeGamePointerAddress) +
            ActiveGamePointerOffset;

        return _memoryReader.ReadInt32(
            activeGameAddress);
    }

    public bool TryGetActiveGame(
        out GameId game)
    {
        game = GetRawValue() switch
        {
            > 99999 => GameId.SonicOriginsMenu,
            1 => GameId.Sonic1,
            2 => GameId.SonicCD,
            3 => GameId.Sonic2,
            4 => GameId.Sonic3AndKnuckles,
            _ => GameId.Unknown
        };

        return game != GameId.Unknown;
    }
}
