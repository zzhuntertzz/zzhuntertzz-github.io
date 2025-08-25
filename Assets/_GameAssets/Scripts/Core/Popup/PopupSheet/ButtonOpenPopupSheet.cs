using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ButtonOpenPopupSheet : MonoBehaviour, IButtonClick, IToggle
{
    [SerializeField] private Transform showPos;
    
    [ValueDropdown("GetFilteredTypeSheetList")]
    [OnValueChanged("OnAssignSheet")]
    [SerializeField] private string PopupSheetName;
    [SerializeField, ReadOnly] private Popup popup;

    private bool isUseToggle = false;
    
    private void Awake()
    {
        isUseToggle = GetComponent<ToggleUI>();
    }

    void Open()
    {
        var sheet = FunctionCommon.GetClass<PopupSheet>(PopupSheetName);
        var method = popup.GetType()
            .GetMethod(nameof(Popup.ShowPopupSheet))
            ?.MakeGenericMethod(sheet);
        method?.Invoke(popup, new[] {new object[1] {showPos}});
    }
    
    public void ButtonClick()
    {
        if (!isUseToggle) Open();
    }
    
    public void ToggleClick()
    {
    }

    public void ToggleOn()
    {
        if (isUseToggle) Open();
    }

    public void ToggleOff()
    {
    }

#if UNITY_EDITOR
    void OnAssignSheet()
    {
        popup = null;
        Transform currentTarget = transform;
        while (popup is null)
        {
            popup = currentTarget.GetComponent<Popup>();
            if (currentTarget.parent is null) return;
            currentTarget = currentTarget.parent;
        }
    }
    
    public IEnumerable GetFilteredTypeSheetList()
    {
        var q = typeof(PopupSheet).Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => typeof(PopupSheet).IsAssignableFrom(x));  
        return q.Select(x => x.FullName);
    }
#endif
}