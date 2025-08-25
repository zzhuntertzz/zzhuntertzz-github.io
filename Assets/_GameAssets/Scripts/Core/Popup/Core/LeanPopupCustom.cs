using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Gui;
using Lean.Transition;
using UnityEngine;

[RequireComponent(typeof(LeanWindow))]
public class LeanPopupCustom : MonoBehaviour, IPopCustom
{
    private LeanWindow _leanWindow;
    private Action onOn, onOff;
    internal float timeTransitionOn, timeTransitionOff;
    private CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {
        _leanWindow = GetComponent<LeanWindow>();
        _leanWindow.OnOn.AddListener(delegate { onOn.Invoke(); });
        _leanWindow.OnOff.AddListener(delegate { onOff.Invoke(); });

        float GetTimeTransition(bool isOn)
        {
            var transition = isOn ?
                transform.Find("[On Transitions]") :
                transform.Find("[Off Transitions]");
            var trans = transition.GetComponents<LeanMethodWithStateAndTarget>();
            float result = 0;
            foreach (var tran in trans)
            {
                object state = tran.GetFieldValue("Data");
                var time = (float)state.GetFieldValue("Duration");
                if (result < time)
                    result = time;
            }

            return result;
        }

        timeTransitionOn = GetTimeTransition(true);
        timeTransitionOff = GetTimeTransition(false);
    }

    public void SetOnAction(string methodName)
    {
        onOn = async delegate
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            await UniTask.Delay((int)(timeTransitionOn * 1000),
                DelayType.UnscaledDeltaTime, cancellationToken: _cancellationTokenSource.Token);
            SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        };
    }
    public void SetOffAction(string methodName)
    {
        onOff = async delegate
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            await UniTask.Delay((int)(timeTransitionOff * 1000),
                DelayType.UnscaledDeltaTime, cancellationToken: _cancellationTokenSource.Token);
            SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        };
    }

    public void Close()
    {
        SendMessage(nameof(Popup.OnClickClose), SendMessageOptions.DontRequireReceiver);
    }
}
