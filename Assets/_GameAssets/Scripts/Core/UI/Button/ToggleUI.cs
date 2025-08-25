using System;
using Lean.Gui;
using UnityEngine;

[RequireComponent(typeof(LeanToggle))]
public class ToggleUI : ButtonUI
{
    private Action<bool> onToggle = delegate(bool isOn) {  };
    private LeanToggle _leanToggle;

    private void Start()
    {
        _leanToggle = GetComponent<LeanToggle>();
        _leanToggle.OnOn.AddListener(delegate
        {
            SendMessage(nameof(ToggleOn), SendMessageOptions.DontRequireReceiver);
        });
        _leanToggle.OnOff.AddListener(delegate
        {
            SendMessage(nameof(ToggleOff), SendMessageOptions.DontRequireReceiver);
        });
    }

    public void SetListener(Action<bool> onToggle)
    {
        this.onToggle = onToggle;
    }

    protected override void ButtonClick()
    {
        base.ButtonClick();
        SendMessage("Toggle", SendMessageOptions.DontRequireReceiver);
    }

    void ToggleOn()
    {
        onToggle(true);
    }

    void ToggleOff()
    {
        onToggle(false);
    }

    public void TriggerOn()
    {
        SendMessage("TurnOn", SendMessageOptions.DontRequireReceiver);
    }

    public void TriggerOff()
    {
        SendMessage("TurnOff", SendMessageOptions.DontRequireReceiver);
    }
}
