using UnityEngine;

public class SavableObject : MonoBehaviour, ISavable
{
    public T GetData<T>(GameObject go) where T : LevelSavableData, new()
    {
        var data = new T();
        data.FromGameObject(go);
        return data;
    }
}