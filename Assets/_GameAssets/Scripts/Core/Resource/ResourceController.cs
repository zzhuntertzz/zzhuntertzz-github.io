using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResourceController : SinglePrivaton<ResourceController>
{
    public enum InitState
    {
        None,
        Progressing,
        Done,
    }
    
    private static List<Action> lstQueue = new();
    public static InitState initState = InitState.None;

    private void Start()
    {
        if (initState != InitState.None) return;

        initState = InitState.Progressing;
        AddressablesManager.Initialize(async delegate
        {
            GameConfig.Instance.Init();
            for (int i = 0; i < lstQueue.Count; i++)
            {
                lstQueue[i].Invoke();
            }
            lstQueue.Clear();
            
            initState = InitState.Done;
        }, delegate
        {
            Debug.Log($"load address fail");

            initState = InitState.None;
        });
    }
    
    public async void AddQueue(Action action)
    {
        if (initState == InitState.Done)
        {
            action();
        }
        else
            lstQueue.Add(action);
    }
}