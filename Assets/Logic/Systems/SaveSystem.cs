using Scellecs.Morpeh;
using UnityEngine;
using System.IO;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class SaveSystem : ISystem
{
    public World World { get; set; }

    private Filter gameStateFilter;
    private Stash<GameStateComponent> gameStateComponents;

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Stash<ElementComponent> elementComponents;

    private Request<SaveRequest> saveRequest;

    public void OnAwake()
    {
        gameStateFilter = World.Filter.With<GameStateComponent>().Build();
        gameStateComponents = World.GetStash<GameStateComponent>();

        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        elementComponents = World.GetStash<ElementComponent>();

        saveRequest = World.GetRequest<SaveRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var request in saveRequest.Consume())
        {
            switch (request.actionType)
            {
                case SaveRequestActionType.SaveLevelIndex:
                    SaveLevelIndex();
                    break;
                case SaveRequestActionType.SaveGrid:
                    SaveGridState();
                    break;
                case SaveRequestActionType.ClearGrid:
                    ClearGridState();
                    break;
            }
        }
    }

    public void SaveLevelIndex()
    {
        if (gameStateFilter.IsEmpty())
            return;

        var gameStateEntity = gameStateFilter.First();
        ref var gameStateComponent = ref gameStateComponents.Get(gameStateEntity);

        var savePath = Helpers.GetSaveFilePath();

        using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            fs.Seek(0, SeekOrigin.Begin);

            writer.Write(gameStateComponent.currentLevelIndex);

            if (fs.Length > sizeof(int))
            {
                fs.Seek(sizeof(int), SeekOrigin.Begin);
                var gridDataLength = new BinaryReader(fs).ReadInt32();
                fs.Seek(sizeof(int) + sizeof(int), SeekOrigin.Begin);

                if (gridDataLength > 0)
                {
                    var gridData = new int[gridDataLength];
                    var reader = new BinaryReader(fs);
                    for (int i = 0; i < gridDataLength; i++)
                    {
                        gridData[i] = reader.ReadInt32();
                    }

                    fs.Seek(0, SeekOrigin.Begin);
                    writer.Write(gameStateComponent.currentLevelIndex);
                    writer.Write(gridDataLength);
                    foreach (int type in gridData)
                    {
                        writer.Write(type);
                    }
                }
            }
        }

        Debug.Log($"Saved level index: {gameStateComponent.currentLevelIndex}");
    }

    public void SaveGridState()
    {
        if (gridFilter.IsEmpty())
            return;

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        var gridData = SerializeGrid(gridComponent);
        var savePath = Helpers.GetSaveFilePath();

        using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            fs.Seek(0, SeekOrigin.Begin);
            int currentLevelIndex = 0;

            if (fs.Length >= sizeof(int))
            {
                currentLevelIndex = new BinaryReader(fs).ReadInt32();
            }

            fs.Seek(0, SeekOrigin.Begin);
            writer.Write(currentLevelIndex);
            writer.Write(gridData.Length);
            foreach (int type in gridData)
            {
                writer.Write(type);
            }
        }

        Debug.Log($"Saved grid state with {gridData.Length} elements");
    }

    private void ClearGridState()
    {
        var savePath = Helpers.GetSaveFilePath();

        if (!File.Exists(savePath))
            return;

        try
        {
            SaveData currentSaveData = new SaveData();
            using (FileStream fs = new FileStream(savePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                currentSaveData.currentLevelIndex = reader.ReadInt32();
                int gridStateLength = reader.ReadInt32();
                for (int i = 0; i < gridStateLength; i++)
                {
                    reader.ReadInt32();
                }
            }

            currentSaveData.gridData = null;

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(currentSaveData.currentLevelIndex);

                writer.Write(0);
            }

            Debug.Log($"Cleared grid data from save: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to clear grid data from save: {e.Message}");
        }
    }

    private int[] SerializeGrid(GridComponent gridComponent)
    {
        int width = gridComponent.state.GetLength(0);
        int height = gridComponent.state.GetLength(1);
        int[] serializedGrid = new int[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var entity = gridComponent.elements[x, y];
                if (entity.HasValue)
                {
                    ref var elementComponent = ref elementComponents.Get(entity.Value);
                    serializedGrid[y * width + x] = elementComponent.type;
                }
                else serializedGrid[y * width + x] = -1;
            }
        }

        return serializedGrid;
    }

    public void Dispose() { }
}