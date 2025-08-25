using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Events;

public abstract class PopupSheet : EventListenerBase, IPopup, IPoolable
{
    [SerializeField] private bool destroyOnHide; 
    public Action onHide = delegate { };
    
    protected override Dictionary<string, UnityAction> GetListEvents()
    {
        return new();
    }

    public virtual void Init()
    {
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

    public virtual void Hide()
    {
        if (isActiveAndEnabled)
            SendMessage("TurnOff", SendMessageOptions.DontRequireReceiver);
        else
            OnHide();
    }

    public virtual void OnHide()
    {
        if (destroyOnHide)
        {
            Destroy(gameObject, .1f);
        }
        else
        {
            gameObject.SetActive(false);
            LeanPool.Despawn(gameObject);
        }
        onHide();
    }

    public virtual void OnSpawn()
    {
    }

    public virtual void OnDespawn()
    {
    }
}
