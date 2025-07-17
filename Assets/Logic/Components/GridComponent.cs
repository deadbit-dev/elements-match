using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]

public struct CellData
{
    public bool is_busy;
    public Entity elementEntity;
}

public struct GridComponent : IComponent
{
    public CellData[,] cells;
}