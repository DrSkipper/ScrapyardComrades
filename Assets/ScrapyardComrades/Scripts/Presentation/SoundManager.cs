using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] AudioSources;
    public SoundEntry[] SoundEntries;

    [System.Serializable]
    public struct SoundEntry
    {
        public AudioClip Clip;
        public int MinFramesBetweenPlays;
    }

    void Awake()
    {
        _instance = this;
        _soundEntries = new Dictionary<string, SoundEntry>(this.SoundEntries.Length);
        _cooldowns = new Dictionary<string, int>(this.SoundEntries.Length);

        for (int i = 0; i < this.SoundEntries.Length; ++i)
        {
            _soundEntries.Add(this.SoundEntries[i].Clip.name, this.SoundEntries[i]);
            _cooldowns.Add(this.SoundEntries[i].Clip.name, 0);
        }
    }

    public static void Play(string clipName)
    {
        _instance.PlayClip(clipName);
    }

    public void PlayClip(string clipName)
    {
        if (_soundEntries.ContainsKey(clipName) && _cooldowns[clipName] <= 0)
        {
            AudioSource source = findAvailableAudioSource();
            if (source != null)
            {
                SoundEntry sfx = _soundEntries[clipName];
                source.clip = sfx.Clip;
                _cooldowns[clipName] = sfx.MinFramesBetweenPlays;
            }
        }
    }

    void FixedUpdate()
    {
        foreach (string key in _cooldowns.Keys)
        {
            if (_cooldowns[key] > 0)
                --_cooldowns[key];
        }
    }

    /**
     * Private
     */
    private static SoundManager _instance;
    private Dictionary<string, SoundEntry> _soundEntries;
    private Dictionary<string, int> _cooldowns;

    private AudioSource findAvailableAudioSource()
    {
        for (int i = 0; i < this.AudioSources.Length; ++i)
        {
            if (!this.AudioSources[i].isPlaying)
                return this.AudioSources[i];
        }
        return null;
    }
}
