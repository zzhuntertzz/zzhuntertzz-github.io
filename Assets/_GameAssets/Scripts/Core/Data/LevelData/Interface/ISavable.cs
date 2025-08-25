using UnityEngine;

public interface ISavable
{
    T GetData<T>(GameObject go) where T : LevelSavableData, new();
}