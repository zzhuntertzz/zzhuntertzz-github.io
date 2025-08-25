using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public static class SoundController
{
    [Serializable]
    private class AudioSourceHandler
    {
        public AudioSource source;
        public string group;

        public AudioSourceHandler(AudioSource source, string group)
        {
            this.source = source;
            this.group = group;
        }

        public void Stop()
        {
            source.Stop();
        }
    }

    private static readonly string NAME_MAIN_MIXER = "Game";

    private static Dictionary<string, Queue<AudioSourceHandler>> _activeSources = new();
    private static Dictionary<string, Stack<AudioSourceHandler>> _restingSources = new();

    public static readonly string GroupSoundFx = "SoundFx";
    public static readonly string GroupSoundBG = "SoundBG";
    
    private static float VOLUME_MAX_BG = 10f;
    private static float VOLUME_MAX_FX = 0f;
    private static float VOLUME_MIN = -80f;

    private static GameObject _soundManager;

    private static GameObject SoundManager
    {
        get
        {
            if (!_soundManager) _soundManager = 
                new GameObject("SoundController",
                typeof(DontDestroyOnLoad));
            return _soundManager;
        }
    }
    
    public static async UniTask<AudioSource> PlayAudio(string clipName,
        bool isLoop = false, bool isUnique = true, string group = "Master", Action onDone = null)
    {
        var sound = await A.Get<AudioClip>(clipName);
        var audio = await CreateAudioSource(sound, isLoop, isUnique, group, onDone);
        // A.Unload(clipName);
        
        return audio;
    }

    public static void StopAudio(string clipName, bool stopAll = true)
    {
        if (!_activeSources.ContainsKey(clipName)) return;
        if (stopAll)
            StopSources(_activeSources[clipName]);
        else
        if (_activeSources[clipName].Count > 0)
            StopSource(_activeSources[clipName].Dequeue());
    }

    private static async UniTask<AudioSource> CreateAudioSource(AudioClip clip,
        bool isLoop = false, bool isUnique = true, string group = "Master", Action onDone = null)
    {
        AudioSourceHandler sourceHandler;
        bool sourceExist = _activeSources.ContainsKey(clip.name);
        if (!sourceExist)
            _activeSources.Add(clip.name, new());
        var queue = _activeSources[clip.name];
        if (isUnique && sourceExist && queue.Count > 0)
        {
            return queue.Peek().source;
        }
        sourceHandler = await GetRestingAudio(group);
        queue.Enqueue(sourceHandler);
        
        AudioSource source = sourceHandler.source;
        
        source.clip = clip;
        source.loop = isLoop;
        if (!isLoop)
        {
            async void RemoveAudio()
            {
                var cancel = new CancellationTokenSource();
                await UniTask.Delay((int)(source.clip.length * 1000),
                    cancellationToken: cancel.Token, delayType: DelayType.UnscaledDeltaTime);
                onDone?.Invoke();
                StopAudio(clip.name, false);
            }
            RemoveAudio();
        }
        source.Play();

        return sourceHandler.source;
    }

    private static async UniTask<AudioSourceHandler> GetRestingAudio(string group)
    {
        AudioSourceHandler sourceHandler = null;
        
        if (_restingSources.Count == 0 ||
            !_restingSources.ContainsKey(group) ||
            _restingSources[group].Count == 0)
        {
            var newAudioSource = SoundManager.AddComponent<AudioSource>();
            var mixer = await GetMixer();
            if (mixer)
            {
                var mixerGroups = mixer.FindMatchingGroups("");
                foreach (var mixerGroup in mixerGroups)
                {
                    if (mixerGroup.name != group) continue;
                    newAudioSource.outputAudioMixerGroup = mixerGroup;
                    break;
                }
            }
            sourceHandler = new AudioSourceHandler(newAudioSource, group);
        }
        else
        {
            sourceHandler = _restingSources[group].Pop();
        }
        
        return sourceHandler;
    }

    private static void StopSources(Queue<AudioSourceHandler> lstSourceHandler)
    {
        for (int i = 0; i < lstSourceHandler.Count; i++)
        {
            StopSource(lstSourceHandler.Dequeue());
        }
    }

    private static void StopSource(AudioSourceHandler sourceHandler)
    {
        sourceHandler.Stop();
        if (!_restingSources.ContainsKey(sourceHandler.group))
            _restingSources.Add(sourceHandler.group, new());
        _restingSources[sourceHandler.group].Push(sourceHandler);
    }

    public static void StopAllSources()
    {
        foreach (var pair in _activeSources)
        {
            StopSources(pair.Value);
        }
    }

    private static async UniTask<AudioMixer> GetMixer()
    {
        return await A.Get<AudioMixer>(NAME_MAIN_MIXER);
    }

    public static void SetMuteGroup(string group, bool isMute)
    {
        var volumeMax = 0f;
        if (group == GroupSoundBG)
            volumeMax = VOLUME_MAX_BG;
        else if (group == GroupSoundFx)
            volumeMax = VOLUME_MAX_FX;
        SetVolumeGroup(group, isMute ? VOLUME_MIN : volumeMax);
    }

    public static async void SetVolumeGroup(string group, float value)
    {
        var mixer = await GetMixer();
        if (!mixer) return;
        mixer.SetFloat($"{group}_Volume", value);
    }
}