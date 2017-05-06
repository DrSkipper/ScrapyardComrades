using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MusicController : MonoBehaviour
{
    public GuaranteeSingleSpawn SingleSpawnCheck;
    public AudioSource AudioSource;
    public bool NoMusicInEditor = false;
    public MusicEntry[] MusicEntries;

    [System.Serializable]
    public struct MusicEntry
    {
        public string SceneName;
        public AudioClip Clip;
    }

#if UNITY_EDITOR
    void Awake()
    {
        if (this.NoMusicInEditor)
        {
            this.AudioSource.Stop();
            this.AudioSource.enabled = false;
            this.enabled = false;
            return;
        }
    }
#endif

    void OnLevelWasLoaded(int i)
    {
        if (!this.SingleSpawnCheck.MarkedForDestruction)
        {
            if (_musicDict == null)
            {
                compileMusicDict();
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (_musicDict.ContainsKey(sceneName) && this.AudioSource.clip != _musicDict[sceneName])
            {
                this.AudioSource.clip = _musicDict[sceneName];
                this.AudioSource.Play();
            }
        }
    }

    /**
     * Private
     */
    private Dictionary<string, AudioClip> _musicDict;

    private void compileMusicDict()
    {
        _musicDict = new Dictionary<string, AudioClip>(this.MusicEntries.Length);

        for (int i = 0; i < this.MusicEntries.Length; ++i)
        {
            _musicDict.Add(this.MusicEntries[i].SceneName, this.MusicEntries[i].Clip);
        }
    }
}
