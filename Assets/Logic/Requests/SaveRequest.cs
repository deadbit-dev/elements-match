using Scellecs.Morpeh;

public enum SaveRequestActionType
{
    SaveLevelIndex,
    SaveGrid,
    ClearGrid,
}

public struct SaveRequest : IRequestData
{
    public SaveRequestActionType actionType;
}