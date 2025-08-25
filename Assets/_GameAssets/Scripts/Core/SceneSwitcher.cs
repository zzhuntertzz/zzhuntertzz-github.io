using System;
using Cysharp.Threading.Tasks;
using Lean.Pool;
using UnityEngine.SceneManagement;

public static class SceneSwitcher
{
    public static async UniTask SwitchScene(string sceneName,
        Action onStart = null, Action onDone = null)
    {
        onStart?.Invoke();
        
        await PopupManager.LoadPopup<LoadingPop>();
        await UniTask.DelayFrame(1);
        
        Clean(sceneName);

        await SwitchSceneWithoutLoadingPop(sceneName);
        onDone?.Invoke();
    }
    public static async UniTask SwitchSceneWithoutLoadingPop(string sceneName)
    {
        await SwitchSceneOnly(sceneName);
        PopupManager.UnLoadAllPopup();
    }
    public static async UniTask SwitchSceneOnly(string sceneName)
    {
        Clean(sceneName);
        bool isDone = false;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
            .completed += _ => isDone = true;
        await UniTask.WaitUntil(() => isDone);
    }

    private static void Clean(string sceneName)
    {
        LeanPool.DespawnAll();
        if (sceneName != SceneManager.GetActiveScene().name)
        {
            A.ClearAllCache();
            // EventManager.EmitEvent(CustomLeanGameObjectPool.EVENT_CLEAN);
        }
        GC.Collect();
    }
}