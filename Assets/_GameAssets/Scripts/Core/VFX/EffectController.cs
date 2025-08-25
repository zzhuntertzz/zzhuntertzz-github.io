using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Lean.Pool;
using UnityEngine;

[Serializable]
public class EffectPackData
{
    public bool isRequesting;
    public List<Transform> lstEffect;
}

public static class EffectController
{
    private static Dictionary<string, EffectPackData> _dicEffects = new();

    public static async UniTask<Transform> ShowEffect(string effectName, Transform showPos,
        Quaternion direction = default)
    {
        var fx = await ShowEffect(effectName, showPos.position, direction);
        if (fx)
        {
            var scale = fx.transform.localScale;
            fx.SetParent(showPos);
            fx.transform.localScale = scale;
        }
        return fx;
    }
    public static async UniTask<Transform> ShowEffect(string effectName, Vector3 showPos,
        Quaternion direction = default, bool unique = false)
    {
        var obj = await A.Get<GameObject>(effectName);

        if (!obj) return null;
        
        EffectPackData GetPackData()
        {
            if (!_dicEffects.ContainsKey(effectName))
                _dicEffects.Add(effectName, new()
                {
                    lstEffect = new(),
                });
            return _dicEffects[effectName];
        }
        
        if (unique)
        {
            if (GetPackData().lstEffect.Count > 0)
                return GetPackData().lstEffect[0];
            if (GetPackData().isRequesting) return null;
        }

        GetPackData().isRequesting = true;
        var effect = LeanPool.Spawn(obj.transform, showPos, direction);
        AddEffectToDic(effectName, effect);
        GetPackData().isRequesting = false;
        
        // A.Unload(effectName);
        return effect;
    }
    public static void HideEffect(string effectName, Transform target = null)
    {
        RemoveEffectFromDic(effectName, target);
    }

    private static void AddEffectToDic(string name, Transform fx)
    {
        if (_dicEffects.ContainsKey(name))
            _dicEffects[name].lstEffect.Add(fx);
        else
            _dicEffects.Add(name, new()
            {
                lstEffect = new()
                {
                    fx,
                },
            });
    }

    private static void RemoveEffectFromDic(string name, Transform target = null)
    {
        if (!_dicEffects.ContainsKey(name)) return;
        var lstFx = _dicEffects[name];
        if (target)
        {
            var fx = lstFx.lstEffect.Find(x => x.Equals(target));
            if (fx)
            {
                LeanPool.Despawn(fx);
                lstFx.lstEffect.Remove(fx);
            }
        }
        else
        {
            foreach (var fx in lstFx.lstEffect)
            {
                LeanPool.Despawn(fx);
            }
            lstFx.lstEffect.Clear();
        }
    }
}
