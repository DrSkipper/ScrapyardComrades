using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public SfxSource[] AudioSources;
    public SoundData SoundData;

#if UNITY_EDITOR
    public List<SoundData.Key> RecentCooldownKeys = new List<SoundData.Key>(); // Exposed for debugging
#endif

    void Awake()
    {
        _instance = this;
        _cooldowns = new Dictionary<SoundData.Key, int>(this.AudioSources.Length);
        _cooldownKeys = new List<SoundData.Key>(this.AudioSources.Length);
    }

    public static void Play(SoundData.Key key, Transform proximityTransform = null)
    {
        if (_instance != null && key != SoundData.Key.NONE)
            _instance.playSoundKey(key, proximityTransform);
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

#if UNITY_EDITOR
        if (Time.frameCount % 500 == 0)
        {
            this.RecentCooldownKeys.Clear();
            this.RecentCooldownKeys.AddRange(_cooldownKeys);
        }
#endif
    }

    /**
     * Private
     */
    private static SoundManager _instance;
    private Dictionary<SoundData.Key, int> _cooldowns;
    private List<SoundData.Key> _cooldownKeys;

    private void playSoundKey(SoundData.Key key, Transform proximityTransform)
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
                        SfxSource source = findAvailableAudioSource();
                        if (source != null)
                        {
                            found = true;
                            source.Play(entry.Clip, entry.Volume, entry.Pitch, entries.UseProximity, proximityTransform, entries.ProximityClose, entries.ProximityFar);
                        }
                    }
                }
                if (found)
                {
                    _cooldowns.Add(key, this.SoundData.CooldownsByEnumIndex[keyIndex]);
                    _cooldownKeys.Add(key);

#if UNITY_EDITOR
                    this.RecentCooldownKeys.AddUnique(key);
#endif
                }
            }
        }
    }

    private SfxSource findAvailableAudioSource()
    {
        for (int i = 0; i < this.AudioSources.Length; ++i)
        {
            if (!this.AudioSources[i].isPlaying)
                return this.AudioSources[i];
        }
        return null;
    }
}
