using Scellecs.Morpeh;

public struct LoadLevelRequest : IRequestData
{
    public int levelIndex;
    public int[] gridData;
}