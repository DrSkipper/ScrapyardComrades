﻿using UnityEngine;
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
        //_soundEntries = new Dictionary<string, SoundEntry>(this.SoundEntries.Length);
        //_cooldowns = new Dictionary<string, int>(this.AudioSources.Length);
        //_cooldownRemovals = new List<string>();
        _cooldowns = new Dictionary<SoundData.Key, int>(this.AudioSources.Length);
        _cooldownKeys = new List<SoundData.Key>(this.AudioSources.Length);

        /*for (int i = 0; i < this.SoundEntries.Length; ++i)
        {
            _soundEntries.Add(this.SoundEntries[i].Clip.name, this.SoundEntries[i]);
            _cooldowns.Add(this.SoundEntries[i].Clip.name, 0);
        }*/
    }

    public static void Play(SoundData.Key key)
    {
        if (_instance != null && key != SoundData.Key.NONE)
            _instance.PlaySoundKey(key);
    }

    /*public static void Play(string clipName)
    {
        _instance.PlayClip(clipName);
    }*/

    public void PlaySoundKey(SoundData.Key key)
    {
        if (!_cooldowns.ContainsKey(key))
        {
            /*AudioSource source = findAvailableAudioSource();
            if (source != null)
            {
                int i = (int)key;
                AudioClip clip = this.SoundData.ClipsByEnumIndex[i];
                float volume = this.SoundData.VolumeByEnumIndex[i];
                float pitch = this.SoundData.PitchByEnumIndex[i];
                int cooldown = this.SoundData.CooldownsByEnumIndex[i];

                source.clip = clip;
                source.volume = volume;
                source.pitch = pitch;
                source.Play();
                _cooldowns.Add(key, cooldown);
                _cooldownKeys.Add(key);
            }*/

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

    /*public void PlayClip(string clipName)
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
                if (!_cooldowns.ContainsKey(clipName))
                    _cooldowns.Add(clipName, sfx.MinFramesBetweenPlays);
                else
                    _cooldowns[clipName] = sfx.MinFramesBetweenPlays;
            }
        }
    }*/

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
    //private Dictionary<string, SoundEntry> _soundEntries;
    //private Dictionary<string, int> _cooldowns;
    private Dictionary<SoundData.Key, int> _cooldowns;
    //private List<string> _cooldownRemovals;
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
