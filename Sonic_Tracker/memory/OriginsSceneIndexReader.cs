namespace Sonic_Tracker.Memory;

public sealed class OriginsSceneIndexReader
{
    private static readonly nint SceneIndexOffset =
        new(0x3FBFFD0);

    private readonly MemoryReader _memoryReader;

    public OriginsSceneIndexReader(
        MemoryReader memoryReader)
    {
        _memoryReader = memoryReader
            ?? throw new ArgumentNullException(nameof(memoryReader));
    }

    public int GetSceneIndex() =>
        _memoryReader.ReadInt32AtModuleOffset(
            SceneIndexOffset);
}
