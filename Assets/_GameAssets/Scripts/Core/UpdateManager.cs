using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class UpdateManager : MonoBehaviour
{
    private static UpdateManager instance;
    private static bool isShuttingDown;

    // Lưu trữ các Action cùng với frame interval và frame count
    private List<(Action callback, int frameInterval, int frameCount)> updateActions = new List<(Action, int, int)>();
    private List<(Action callback, int frameInterval, int frameCount)> fixedUpdateActions = new List<(Action, int, int)>();
    private List<(Action callback, int frameInterval, int frameCount)> lateUpdateActions = new List<(Action, int, int)>();
    private NativeArray<int> updateIndices;
    private NativeArray<bool> updateExecuteFlags;
    private NativeArray<int> fixedUpdateIndices;
    private NativeArray<bool> fixedUpdateExecuteFlags;
    private NativeArray<int> lateUpdateIndices;
    private NativeArray<bool> lateUpdateExecuteFlags;
    private JobHandle updateJobHandle;
    private JobHandle fixedUpdateJobHandle;
    private JobHandle lateUpdateJobHandle;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static UpdateManager Instance
    {
        get
        {
            if (instance == null && !isShuttingDown)
            {
                instance = FindObjectOfType<UpdateManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UpdateManager");
                    instance = go.AddComponent<UpdateManager>();
                }
            }
            return instance;
        }
    }

    public static void SubscribeToUpdate(Action callback, int frameInterval = 1)
    {
        if (Instance == null || isShuttingDown || callback == null) return;
        if (!Instance.updateActions.Any(x => x.callback == callback))
        {
            Instance.updateActions.Add((callback, Mathf.Max(1, frameInterval), 0));
        }
    }

    public static void SubscribeToFixedUpdate(Action callback, int frameInterval = 1)
    {
        if (Instance == null || isShuttingDown || callback == null) return;
        if (!Instance.fixedUpdateActions.Any(x => x.callback == callback))
        {
            Instance.fixedUpdateActions.Add((callback, Mathf.Max(1, frameInterval), 0));
        }
    }

    public static void SubscribeToLateUpdate(Action callback, int frameInterval = 1)
    {
        if (Instance == null || isShuttingDown || callback == null) return;
        if (!Instance.lateUpdateActions.Any(x => x.callback == callback))
        {
            Instance.lateUpdateActions.Add((callback, Mathf.Max(1, frameInterval), 0));
        }
    }

    public static void UnsubscribeFromUpdate(Action callback)
    {
        if (Instance == null || isShuttingDown) return;
        Instance.updateActions.RemoveAll(x => x.callback == callback);
    }

    public static void UnsubscribeFromFixedUpdate(Action callback)
    {
        if (Instance == null || isShuttingDown) return;
        Instance.fixedUpdateActions.RemoveAll(x => x.callback == callback);
    }

    public static void UnsubscribeFromLateUpdate(Action callback)
    {
        if (Instance == null || isShuttingDown) return;
        Instance.lateUpdateActions.RemoveAll(x => x.callback == callback);
    }

    public static void AddItem(MonoBehaviour behaviour)
    {
        if (behaviour == null) throw new NullReferenceException("The behaviour you've tried to add is null!");
        if (Instance == null || isShuttingDown) return;

        Instance.updateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.fixedUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.lateUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
    }

    public static void RemoveSpecificItem(MonoBehaviour behaviour)
    {
        if (behaviour == null) throw new NullReferenceException("The behaviour you've tried to remove is null!");
        if (Instance == null || isShuttingDown) return;

        Instance.updateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.fixedUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.lateUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
    }

    public static void RemoveSpecificItemAndDestroyComponent(MonoBehaviour behaviour)
    {
        if (behaviour == null) throw new NullReferenceException("The behaviour you've tried to remove is null!");
        if (Instance == null || isShuttingDown) return;

        Instance.updateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.fixedUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.lateUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Destroy(behaviour);
    }

    public static void RemoveSpecificItemAndDestroyGameObject(MonoBehaviour behaviour)
    {
        if (behaviour == null) throw new NullReferenceException("The behaviour you've tried to remove is null!");
        if (Instance == null || isShuttingDown) return;

        Instance.updateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.fixedUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Instance.lateUpdateActions.RemoveAll(x => x.callback.Target == behaviour);
        Destroy(behaviour.gameObject);
    }

    private void Update()
    {
        if (updateActions.Count == 0) return;

        // Tăng frame count và kiểm tra interval
        for (int i = 0; i < updateActions.Count; i++)
        {
            var (callback, frameInterval, frameCount) = updateActions[i];
            updateActions[i] = (callback, frameInterval, frameCount + 1);
        }

        // Tạo NativeArray cho Update Actions
        updateIndices = new NativeArray<int>(updateActions.Count, Allocator.TempJob);
        updateExecuteFlags = new NativeArray<bool>(updateActions.Count, Allocator.TempJob);

        for (int i = 0; i < updateActions.Count; i++)
        {
            updateIndices[i] = i;
            updateExecuteFlags[i] = updateActions[i].frameCount % updateActions[i].frameInterval == 0;
        }

        // Chạy Job để đánh dấu Action
        var updateJob = new UpdateJob
        {
            indices = updateIndices,
            executeFlags = updateExecuteFlags,
            count = updateActions.Count
        };
        updateJobHandle = updateJob.Schedule(updateActions.Count, 64); // Tăng batch size để tối ưu
        updateJobHandle.Complete();

        // Gọi Action dựa trên executeFlags
        for (int i = 0; i < updateActions.Count; i++)
        {
            if (updateExecuteFlags[i])
            {
                updateActions[i].callback?.Invoke();
            }
        }

        // Giải phóng NativeArray
        if (updateIndices.IsCreated) updateIndices.Dispose();
        if (updateExecuteFlags.IsCreated) updateExecuteFlags.Dispose();
    }

    private void FixedUpdate()
    {
        if (fixedUpdateActions.Count == 0) return;

        // Tăng frame count và kiểm tra interval
        for (int i = 0; i < fixedUpdateActions.Count; i++)
        {
            var (callback, frameInterval, frameCount) = fixedUpdateActions[i];
            fixedUpdateActions[i] = (callback, frameInterval, frameCount + 1);
        }

        // Tạo NativeArray cho FixedUpdate Actions
        fixedUpdateIndices = new NativeArray<int>(fixedUpdateActions.Count, Allocator.TempJob);
        fixedUpdateExecuteFlags = new NativeArray<bool>(fixedUpdateActions.Count, Allocator.TempJob);

        for (int i = 0; i < fixedUpdateActions.Count; i++)
        {
            fixedUpdateIndices[i] = i;
            fixedUpdateExecuteFlags[i] = fixedUpdateActions[i].frameCount % fixedUpdateActions[i].frameInterval == 0;
        }

        // Chạy Job để đánh dấu Action
        var fixedUpdateJob = new FixedUpdateJob
        {
            indices = fixedUpdateIndices,
            executeFlags = fixedUpdateExecuteFlags,
            count = fixedUpdateActions.Count
        };
        fixedUpdateJobHandle = fixedUpdateJob.Schedule(fixedUpdateActions.Count, 64);
        fixedUpdateJobHandle.Complete();

        // Gọi Action dựa trên executeFlags
        for (int i = 0; i < fixedUpdateActions.Count; i++)
        {
            if (fixedUpdateExecuteFlags[i])
            {
                fixedUpdateActions[i].callback?.Invoke();
            }
        }

        // Giải phóng NativeArray
        if (fixedUpdateIndices.IsCreated) fixedUpdateIndices.Dispose();
        if (fixedUpdateExecuteFlags.IsCreated) fixedUpdateExecuteFlags.Dispose();
    }

    private void LateUpdate()
    {
        if (lateUpdateActions.Count == 0) return;

        // Tăng frame count và kiểm tra interval
        for (int i = 0; i < lateUpdateActions.Count; i++)
        {
            var (callback, frameInterval, frameCount) = lateUpdateActions[i];
            lateUpdateActions[i] = (callback, frameInterval, frameCount + 1);
        }

        // Tạo NativeArray cho LateUpdate Actions
        lateUpdateIndices = new NativeArray<int>(lateUpdateActions.Count, Allocator.TempJob);
        lateUpdateExecuteFlags = new NativeArray<bool>(lateUpdateActions.Count, Allocator.TempJob);

        for (int i = 0; i < lateUpdateActions.Count; i++)
        {
            lateUpdateIndices[i] = i;
            lateUpdateExecuteFlags[i] = lateUpdateActions[i].frameCount % lateUpdateActions[i].frameInterval == 0;
        }

        // Chạy Job để đánh dấu Action
        var lateUpdateJob = new LateUpdateJob
        {
            indices = lateUpdateIndices,
            executeFlags = lateUpdateExecuteFlags,
            count = lateUpdateActions.Count
        };
        lateUpdateJobHandle = lateUpdateJob.Schedule(lateUpdateActions.Count, 64);
        lateUpdateJobHandle.Complete();

        // Gọi Action dựa trên executeFlags
        for (int i = 0; i < lateUpdateActions.Count; i++)
        {
            if (lateUpdateExecuteFlags[i])
            {
                lateUpdateActions[i].callback?.Invoke();
            }
        }

        // Giải phóng NativeArray
        if (lateUpdateIndices.IsCreated) lateUpdateIndices.Dispose();
        if (lateUpdateExecuteFlags.IsCreated) lateUpdateExecuteFlags.Dispose();
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
        if (updateIndices.IsCreated) updateIndices.Dispose();
        if (updateExecuteFlags.IsCreated) updateExecuteFlags.Dispose();
        if (fixedUpdateIndices.IsCreated) fixedUpdateIndices.Dispose();
        if (fixedUpdateExecuteFlags.IsCreated) fixedUpdateExecuteFlags.Dispose();
        if (lateUpdateIndices.IsCreated) lateUpdateIndices.Dispose();
        if (lateUpdateExecuteFlags.IsCreated) lateUpdateExecuteFlags.Dispose();
    }
}

// Job cho Update
public struct UpdateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> indices;
    [NativeDisableParallelForRestriction] public NativeArray<bool> executeFlags;
    public int count;

    public void Execute(int index)
    {
        if (index < count)
        {
            // Không cần thay đổi executeFlags, đã được thiết lập trước
        }
    }
}

// Job cho FixedUpdate
public struct FixedUpdateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> indices;
    [NativeDisableParallelForRestriction] public NativeArray<bool> executeFlags;
    public int count;

    public void Execute(int index)
    {
        if (index < count)
        {
            // Không cần thay đổi executeFlags, đã được thiết lập trước
        }
    }
}

// Job cho LateUpdate
public struct LateUpdateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> indices;
    [NativeDisableParallelForRestriction] public NativeArray<bool> executeFlags;
    public int count;

    public void Execute(int index)
    {
        if (index < count)
        {
            // Không cần thay đổi executeFlags, đã được thiết lập trước
        }
    }
}