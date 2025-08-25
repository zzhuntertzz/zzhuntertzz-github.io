using System;
using Lean.Touch;
using UnityEngine;

[RequireComponent(typeof(LeanEvent),
    typeof(LeanSelectableByFinger))]
public class ButtonWorldClick : MonoBehaviour, IEventSelect
{
    protected bool isInteractable = true;
    private Action onSelect, onDeselect;
    public void SetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }
    public void SetListener(Action onSelect, Action onDeselect = null)
    {
        this.onSelect = onSelect;
        this.onDeselect = onDeselect;
    }
    
    public virtual void OnActionSelected()
    {
        if (!isInteractable) return;
        // Debug.Log($">>Select");
        onSelect?.Invoke();
        
        SoundController.PlayAudio(MyKeys.Sounds.ClickButton,
            group: SoundController.GroupSoundFx);
    }

    public virtual void OnActionDeselected()
    {
        if (!isInteractable) return;
        // Debug.Log($">>DeSelect");
        onDeselect?.Invoke();
    }
}
