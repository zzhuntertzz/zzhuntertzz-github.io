using System;
using Lean.Gui;
using UnityEngine;

[RequireComponent(typeof(LeanButton))]
public class ButtonUI : MonoBehaviour
{
    private readonly string ACTION_BUTTON_CLICK = nameof(ButtonClick);
    
    private Action onClick = delegate { };
    private Action onClickOneTime = delegate { };
    private LeanButton _leanButton;

    protected virtual void Awake()
    {
        _leanButton = GetComponent<LeanButton>();
        _leanButton.OnClick.AddListener(delegate
        {
            SendMessage(ACTION_BUTTON_CLICK, SendMessageOptions.DontRequireReceiver);
        });
    }

    public void SetListener(Action onClick)
    {
        this.onClick = onClick;
    }

    public void SetListenerOneTime(Action onClick)
    {
        this.onClickOneTime = onClick;
    }

    protected virtual void ButtonClick()
    {
        SoundController.PlayAudio(MyKeys.Sounds.ClickButton, group: SoundController.GroupSoundFx);
        onClick();
        onClickOneTime();
        onClickOneTime = delegate { };
    }

    public virtual void SetInteractable(bool isInteractable)
    {
        if (_leanButton)
            _leanButton.interactable = isInteractable;
    }

    public virtual bool IsInteractable()
    {
        if (_leanButton)
            return _leanButton.interactable;
        return false;
    }

    public LeanButton GetButton()
    {
        return _leanButton;
    }
}
