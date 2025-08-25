using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Events;

public abstract class Popup : EventListenerBase, IPopup
{
    [SerializeField] internal bool destroyOnHide, isPauseOnShow;   
    [SerializeField] protected ButtonUI btnClose;

    public Action onHide = delegate { }; 
    
    private PopupSheet _activeSheets = null;
    private CancellationTokenSource _cancelLoadSheet;

    protected override Dictionary<string, UnityAction> GetListEvents()
    {
        return new();
    }
    
    public virtual void Init()
    {
        if (btnClose)
            btnClose.SetListener(OnClickClose);
        SendMessage(nameof(IPopCustom.SetOnAction),
            nameof(OnShow), SendMessageOptions.DontRequireReceiver);
        SendMessage(nameof(IPopCustom.SetOffAction),
            nameof(OnHide), SendMessageOptions.DontRequireReceiver);
    }

    public virtual void Show(params object[] objects)
    {
        gameObject.SetActive(true);
        SendMessage("TurnOn", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void OnShow()
    {
    }

    public virtual void OnClickClose()
    {
        PopupManager.UnloadPopup();
    }

    public virtual void Hide()
    {
        if (isActiveAndEnabled)
            SendMessage("TurnOff", SendMessageOptions.DontRequireReceiver);
        else
            OnHide();
    }

    public virtual void OnHide()
    {
        UnloadSheet(true);
        CancelLoadSheet();
        if (destroyOnHide)
        {
            Destroy(gameObject, .1f);
        }
        else
        {
            gameObject.SetActive(false);
        }
        onHide();
    }

    public virtual async UniTask<T> ShowPopupSheet<T>(params object[] objects) where T : PopupSheet
    {
        var sheetName = $"{typeof(T).Name}";

        var sheetExist = _activeSheets is not null && _activeSheets.GetType().Name == sheetName;
        if (sheetExist)
        {
            return sheetExist as T;
        }
        
        UnloadSheet();
        CancelLoadSheet();
        _cancelLoadSheet = new();
        var task = A.Get<GameObject>(sheetName);
        var obj = await task.AttachExternalCancellation(_cancelLoadSheet.Token);
        if (obj is null) return null;
        
        var pop = LeanPool.Spawn(obj,
            objects.Length > 0 && objects[0] is Transform trans ?
            trans : transform);
        var popType = pop.GetComponent<T>();
        popType.Init();
        
        _activeSheets = popType;
        
        popType.Show(objects);
        
        return popType;
    }

    void CancelLoadSheet()
    {
        if (_cancelLoadSheet is not null)
        {
            _cancelLoadSheet.Cancel();
            _cancelLoadSheet = null;
        }
    }

    void UnloadSheet(bool forceHide = false)
    {
        if (_activeSheets is not null)
        {
            if (forceHide) _activeSheets.gameObject.SetActive(false);
            _activeSheets.Hide();
            _activeSheets = null;
        }
    }
}