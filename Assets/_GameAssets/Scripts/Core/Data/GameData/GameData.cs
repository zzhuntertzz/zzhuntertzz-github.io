using System;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

[Searchable]
[CreateAssetMenu(menuName = "ScriptableObjects/GameData", fileName = "GameData")]
public partial class GameData : SerializedScriptableObject
{
    private static GameData _instance;
    
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Debug.Log($">>Init Data {GameData.Instance}");;
    }

    public static GameData Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = Resources.Load<GameData>(nameof(GameData));
                _instance?.InitAllData();
            }
            return _instance;
        }
    }

    private void InitAllData()
    {
        foreach (var field in typeof(GameData).GetFields())
        {
            var parseMethod = field.FieldType.GetMethod(
                nameof(IDataPublic.InitData), Type.EmptyTypes);
            if (parseMethod != null)
            {
                var obj = field.GetValue(this);
                parseMethod.Invoke(obj,null);
            }
        }
    }
    
    [Button]
    public virtual void LoadAllGSheet()
    {
        foreach (var field in typeof(GameData).GetFields())
        {
            var parseMethod = field.FieldType.GetMethod(
                nameof(IDataGSheet.LoadGSheet), Type.EmptyTypes);
            if (parseMethod != null)
            {
                var obj = field.GetValue(this);
                parseMethod.Invoke(obj,null);
            }
        }
    }
    
    [Button,GUIColor(1,0,1)]
    public static void ClearData()
    {
        _instance = null;
        PlayerPrefs.DeleteAll();
    }
}

#if UNITY_EDITOR

public class GameDataWindow : OdinEditorWindow
{
    private GameData _gameData;
    [InlineEditor(InlineEditorObjectFieldModes.Hidden), ShowInInspector]
    private GameData GameData
    {
        get=>_gameData ??= Resources.Load<GameData>(nameof(GameData));
        set => _gameData = value;
    } 
    
    [MenuItem("Window/GameData")]
    public static void Show()
    {
        ((EditorWindow)GetWindow<GameDataWindow>()).Show();
    }
}


#endif
