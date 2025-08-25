using Lean.Touch;
using UnityEngine;

[RequireComponent(typeof(LeanSelectableByFinger))]
public class LeanEvent : MonoBehaviour
{
    private LeanSelectableByFinger _leanSelectableByFinger;

    private void Awake()
    {
        _leanSelectableByFinger = GetComponent<LeanSelectableByFinger>();
        _leanSelectableByFinger.OnSelected.AddListener(delegate
        {
            SendMessage(nameof(IEventSelect.OnActionSelected), SendMessageOptions.DontRequireReceiver);
        });
        _leanSelectableByFinger.OnDeselected.AddListener(delegate
        {
            SendMessage(nameof(IEventSelect.OnActionDeselected), SendMessageOptions.DontRequireReceiver);
        });
    }
}