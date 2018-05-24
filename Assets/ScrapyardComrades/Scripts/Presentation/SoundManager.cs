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
        _soundEntries = new Dictionary<string, SoundEntry>(this.SoundEntries.Length);
        _cooldowns = new Dictionary<string, int>(this.SoundEntries.Length);
        _currentCooldowns = new List<string>(this.SoundEntries.Length);

        for (int i = 0; i < this.SoundEntries.Length; ++i)
        {
            _soundEntries.Add(this.SoundEntries[i].Clip.name, this.SoundEntries[i]);
            _cooldowns.Add(this.SoundEntries[i].Clip.name, 0);
        }
    }

    public static void Play(SoundData.Key key)
    {
        _instance.PlaySoundKey(key);
    }

    public static void Play(string clipName)
    {
        _instance.PlayClip(clipName);
    }

    public void PlaySoundKey(SoundData.Key key)
    {

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
                source.volume = sfx.Volume;
                source.Play();
                _cooldowns[clipName] = sfx.MinFramesBetweenPlays;
                if (!_currentCooldowns.Contains(clipName))
                    _currentCooldowns.Add(clipName);
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < _currentCooldowns.Count;)
        {
            string key = _currentCooldowns[i];
            --_cooldowns[key];
            if (_cooldowns[key] <= 0)
                _currentCooldowns.RemoveAt(i);
            else
                ++i;
        }
    }

    /**
     * Private
     */
    private static SoundManager _instance;
    private Dictionary<string, SoundEntry> _soundEntries;
    private Dictionary<string, int> _cooldowns;
    private List<string> _currentCooldowns;

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
