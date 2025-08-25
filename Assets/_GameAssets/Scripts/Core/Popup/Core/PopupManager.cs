using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class PopupManager
{
    public static event Action<int, Popup> OnPopupCountChange = delegate {  };

    private static readonly Vector2Int SCREEN_SIZE = new(1920, 1080);
    
    private static CanvasGroup _canvasGroup;
    
    private static List<string> _loadingPops = new();
    private static List<Popup> _activePops = new();
    private static Dictionary<string, Popup> _inActivePops = new();
    private static int pausePopShowing = 0;

    public static Transform transform;

    static PopupManager()
    {
        transform = GameObject.Find(nameof(PopupManager))?.transform;
        if (transform is null)
        {
            var go = new GameObject(nameof(PopupManager),
                typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup),
                typeof(CamFinderForCanvas), typeof(DontDestroyOnLoad));
            transform = go.transform;

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = FunctionCommon.mainCam;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 99;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
        
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SCREEN_SIZE;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        
        _canvasGroup = transform.GetComponent<CanvasGroup>();
        ShowCanvas(false, 0);
    }

    public static async UniTask<T> LoadPopup<T>(params object[] objects) where T : Popup
    {
        var popName = $"{typeof(T).Name}";

        await UniTask.WaitUntil(() => !_loadingPops.Contains(popName));
        await UniTask.WaitUntil(() => ResourceController.initState == ResourceController.InitState.Done);
        
        var popExist = _activePops.Find(x => x.GetType().Name == popName);
        if (popExist is not null)
        {
            await UniTask.DelayFrame(1);
            popExist.OnHide();
            popExist.Show(objects);
            return popExist as T;
        }
        
        _loadingPops.Add(popName);
        var obj = await A.Get<GameObject>(popName);
        _loadingPops.Remove(popName);
        if (obj is null) return null;
        
        GameObject pop = null;
        T popType = default;
        if (_inActivePops.ContainsKey(popName))
        {
            popType = _inActivePops[popName] as T;
            _inActivePops.Remove(popName);
            popType.transform.SetAsLastSibling();
        }
        else
        {
            pop = Object.Instantiate(obj, transform);
            popType = pop.GetComponent<T>();
            popType.Init();
        }
        
        _activePops.Add(popType);

        if (popType.isPauseOnShow)
            pausePopShowing++;
        
        OnPopupCountChange(_activePops.Count, popType);
        
        popType.Show(objects);
        CheckPause();
        // A.Unload<T>();
        
        ShowCanvas(true);
        
        return popType;
    }

    public static async UniTask<T> SwitchPopup<T>(params object[] objects) where T : Popup
    {
        if (_activePops.Count > 0)
        {
            if (_activePops[^1] is T) return _activePops[^1] as T;
            UnloadPopup();
        }

        return await LoadPopup<T>(objects);
    }

    public static void HidePopup<T>()
    {
        var popExist = _activePops.Find(
            x => x.GetType().Name == typeof(T).Name);
        if (!popExist) return;
        HidePop(popExist);
    }

    public static void UnLoadAllPopup()
    {
        while (_activePops.Count > 0)
        {
            UnloadPopup();
        }
    }

    public static void UnLoadAllPopupExcept<T>()
    {
        var popType = _activePops.Find(x => x is T);
        if (popType == null)
        {
            UnLoadAllPopup();
            return;
        }

        var lstTmp = new List<Popup>(_activePops);
        foreach (var pop in lstTmp)
        {
            if (pop == popType) continue;
            HidePop(pop);
        }
    }

    public static void UnloadPopup()
    {
        if (_activePops.Count == 0) return;
        var popType = _activePops[^1];
        HidePop(popType);
    }

    private static void HidePop(Popup popType)
    {
        popType.Hide();
        _activePops.Remove(popType);
        if (popType.isPauseOnShow)
            pausePopShowing--;
        CheckPause();
        
        popType.transform.SetAsFirstSibling();

        var popName = popType.GetType().Name;
        if (!popType.destroyOnHide)
        {
            if (_inActivePops.ContainsKey(popName))
                _inActivePops[popName] = popType;
            else
                _inActivePops.Add(popName, popType);
        }

        if (_activePops.Count == 0)
        {
            ShowCanvas(false);
        }
        
        OnPopupCountChange(_activePops.Count, popType);
    }

    public static bool IsShowing<T>()
    {
        var popExist = _activePops.Find(x => x.GetType().Name == typeof(T).Name);
        return _activePops.Contains(popExist);
    }

    public static T GetPopup<T>(string popupName = "") where T : Popup
    {
        if (string.IsNullOrEmpty(popupName))
            popupName = typeof(T).Name;
        return _activePops.Find(
            x => x is T && x.GetType().Name == popupName) as T;
    }

    static void ShowCanvas(bool isShow, float duration = .3f)
    {
        // _canvasGroup.DOKill();
        // _canvasGroup.DOFade(isShow ? 1 : 0, duration).SetUpdate(true);
        _canvasGroup.blocksRaycasts = isShow;
    }

    public static int CurrentPopCount()
    {
        return _activePops.Count;
    }

    private static void CheckPause()
    {
        Time.timeScale = pausePopShowing > 0 ? 0 : 1;
    }
}