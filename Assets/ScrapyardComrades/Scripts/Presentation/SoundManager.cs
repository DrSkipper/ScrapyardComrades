using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] AudioSources;
    public SoundData SoundData;
    public SoundEntry[] SoundEntries;

    [System.Serializable]
    public struct SoundEntry
    {
        public AudioClip Clip;
        public float Volume;
        public int MinFramesBetweenPlays;
    }

    void Awake()
    {
        _instance = this;
        _cooldowns = new Dictionary<SoundData.Key, int>(this.AudioSources.Length);
        _cooldownKeys = new List<SoundData.Key>(this.AudioSources.Length);
    }

    public static void Play(SoundData.Key key)
    {
        if (_instance != null && key != SoundData.Key.NONE)
            _instance.PlaySoundKey(key);
    }

    public void PlaySoundKey(SoundData.Key key)
    {
        if (!_cooldowns.ContainsKey(key))
        {
            int keyIndex = (int)key;
            SoundData.EntryList entries = this.SoundData.EntriesByEnumIndex[keyIndex];
            if (entries != null)
            {
                bool found = false;
                for (int i = 0; i < entries.Count; ++i)
                {
                    SoundData.Entry entry = entries.Entries[i];
                    if (entry.Clip != null)
                    {
                        AudioSource source = findAvailableAudioSource();
                        if (source != null)
                        {
                            found = true;
                            source.clip = entry.Clip;
                            source.volume = entry.Volume;
                            source.pitch = entry.Pitch;
                            source.Play();
                        }
                    }
                }
                if (found)
                {
                    _cooldowns.Add(key, this.SoundData.CooldownsByEnumIndex[keyIndex]);
                    _cooldownKeys.Add(key);
                }
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = _cooldownKeys.Count - 1; i >= 0; --i)
        {
            SoundData.Key key = _cooldownKeys[i];
            --_cooldowns[key];
            if (_cooldowns[key] <= 0)
            {
                _cooldowns.Remove(key);
                _cooldownKeys.RemoveAt(i);
            }
        }
    }

    /**
     * Private
     */
    private static SoundManager _instance;
    private Dictionary<SoundData.Key, int> _cooldowns;
    private List<SoundData.Key> _cooldownKeys;

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
