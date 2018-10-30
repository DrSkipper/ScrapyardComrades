using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public SfxSource[] AudioSources;
    public SoundData SoundData;
    public float GlobalSfxVolumeScale = 1.0f;

#if UNITY_EDITOR
    public List<SoundData.Key> RecentCooldownKeys = new List<SoundData.Key>(); // Exposed for debugging
#endif

    void Awake()
    {
        _instance = this;
        _cooldowns = new Dictionary<SoundData.Key, int>(this.AudioSources.Length);
        _cooldownKeys = new List<SoundData.Key>(this.AudioSources.Length);
    }

    public static void Play(SoundData.Key key, Transform proximityTransform = null, float volumeMultiplier = -1, float pitchMultiplier = -1)
    {
        if (_instance != null && key != SoundData.Key.NONE)
            _instance.playSoundKey(key, proximityTransform, volumeMultiplier, pitchMultiplier);
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

    private void playSoundKey(SoundData.Key key, Transform proximityTransform, float volumeMult, float pitchMult)
    {
        if (!_cooldowns.ContainsKey(key))
        {
            int keyIndex = (int)key;
            SoundData.EntryList entries = this.SoundData.EntriesByEnumIndex[keyIndex];
            if (entries != null)
            {
                bool found = false;
                switch (entries.MultiSfxType)
                {
                    default:
                    case SoundData.MultiSfxBehavior.PlayAll:
                        for (int i = 0; i < entries.Count; ++i)
                        {
                            found |= playEntry(entries.Entries[i], entries, proximityTransform, volumeMult * this.GlobalSfxVolumeScale, pitchMult);
                        }
                        break;

                    case SoundData.MultiSfxBehavior.Randomize:
                        if (entries.Count > 0)
                        {
                            found = playEntry(entries.Entries[Random.Range(0, entries.Entries.Count - 1)], entries, proximityTransform, volumeMult * this.GlobalSfxVolumeScale, pitchMult);
                        }
                        break;
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

    private bool playEntry(SoundData.Entry entry, SoundData.EntryList entries, Transform proximityTransform, float volumeMult, float pitchMult)
    {
        if (entry.Clip != null)
        {
            SfxSource source = findAvailableAudioSource();
            if (source != null)
            {
                float volume = entry.Volume;
                float pitch = entry.Pitch;

                if (volumeMult >= 0.0f)
                    volume = Mathf.Min(1.0f, volume * volumeMult);
                if (pitchMult >= 0.0f)
                    pitch = Mathf.Min(1.0f, pitch * pitchMult);

                source.Play(entry.Clip, volume, pitch, entries.UseProximity, proximityTransform, entries.ProximityClose, entries.ProximityFar);
                return true;
            }
        }
        return false;
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
