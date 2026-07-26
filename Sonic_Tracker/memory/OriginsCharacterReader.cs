using Sonic_Tracker.Games;

namespace Sonic_Tracker.Memory;

public sealed class OriginsCharacterReader
{
    private static readonly nint CharacterPointerModuleOffset =
        new(0x3FC0110);

    private static readonly nint CharacterPointerOffset =
        new(0x14);

    private readonly MemoryReader _memoryReader;
    private readonly nint _characterPointerAddress;

    public OriginsCharacterReader(
        MemoryReader memoryReader)
    {
        _memoryReader = memoryReader
            ?? throw new ArgumentNullException(
                nameof(memoryReader));

        _characterPointerAddress =
            _memoryReader.ResolveModuleOffset(
                CharacterPointerModuleOffset);
    }

    public byte GetRawValue()
    {
        nint characterAddress =
            _memoryReader.ReadPointer(
                _characterPointerAddress) +
            CharacterPointerOffset;

        return _memoryReader.ReadByte(
            characterAddress);
    }

    public bool TryGetLeadCharacter(
        out PlayableCharacter character)
    {
        switch (GetRawValue())
        {
            case 0:
            case 3:
                character =
                    PlayableCharacter.Sonic;
                return true;

            case 1:
                character =
                    PlayableCharacter.Tails;
                return true;

            case 2:
            case 4:
                character =
                    PlayableCharacter.Knuckles;
                return true;

            case 5:
            case 6:
                character =
                    PlayableCharacter.Amy;
                return true;

            default:
                character = default;
                return false;
        }
    }
}
