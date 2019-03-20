using UnityEngine;
using System.Collections.Generic;

public class GameplayMusicController : MonoBehaviour
{
    public WorldLoadingManager LoadingManager;
    public AudioSource AudioSource;
    public VolumeFader VolumeFader;
    public int FadeOutDuration = 48;
    public bool NoMusicInEditor = false;
    public GameplayMusicDefault[] GameplayMusicDefaults;

    [System.Serializable]
    public class GameplayMusicDefault
    {
        public string RoomPrefix;
        public float Volume;
        public AudioClip Clip; //TODO: array of clips by layer
    }

    //TODO:
    /*
    [System.Serializable]
    public class GameplayMusicRoomLayers
    {
        public string[] RoomNames;
        public int[] LayersActive;
    }
    */

    private Dictionary<string, GameplayMusicDefault> _defaults;
    private string _prevRoomName;
    private string _prevRoomPrefix;
    private bool _switchingTracks;
    private GameplayMusicDefault _prevEntry;

    void Awake()
    {
#if UNITY_EDITOR
        if (this.NoMusicInEditor)
        {
            this.AudioSource.Stop();
            this.AudioSource.enabled = false;
            this.enabled = false;
            return;
        }
#endif
        _defaults = new Dictionary<string, GameplayMusicDefault>(this.GameplayMusicDefaults.Length);
        for (int i = 0; i < this.GameplayMusicDefaults.Length; ++i)
        {
            GameplayMusicDefault entry = this.GameplayMusicDefaults[i];
            _defaults.Add(entry.RoomPrefix, entry);
        }

        GlobalEvents.Notifier.Listen(BeginSceneTransitionEvent.NAME, this, beginSceneTransition);
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, onResume);
    }

    private void Start()
    {
        _prevRoomName = this.LoadingManager.CurrentQuadName;
        _prevRoomPrefix = getRoomPrefix(_prevRoomName);
        if (_defaults.ContainsKey(_prevRoomPrefix))
        {
            //TODO: start music layers based on specific room
            _prevEntry = _defaults[_prevRoomPrefix];
            if (_prevEntry.Clip != null)
            {
                this.AudioSource.clip = _prevEntry.Clip;
                this.AudioSource.volume = _prevEntry.Volume;
                this.AudioSource.Play();
            }
        }
    }

    private void beginSceneTransition(LocalEventNotifier.Event e)
    {
        string nextSceneName = (e as BeginSceneTransitionEvent).NextSceneName;
        this.VolumeFader.BeginFade(this.FadeOutDuration, 0.0f);
    }

    // Starting room transition
    private void onPause(LocalEventNotifier.Event e)
    {
        PauseEvent pe = e as PauseEvent;
        if (pe.PauseGroup == PauseController.PauseGroup.SequencedPause && pe.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            //TODO: if it hasn't check if the specific room name has a different music layer set than the previous one
            string nextRoomName = this.LoadingManager.CurrentQuadName;
            string nextPrefix = getRoomPrefix(nextRoomName);
            if (_prevRoomPrefix != nextPrefix && nextRoomName.ToLower() != "slums 12") //TODO: Remove hack
            {
                if (_defaults.ContainsKey(nextPrefix))
                {
                    GameplayMusicDefault nextDefault = _defaults[nextPrefix];
                    if (nextDefault != _prevEntry)
                    {
                        if (_prevEntry == null || nextDefault.Clip != _prevEntry.Clip)
                        {
                            // Switching tracks
                            _switchingTracks = true;
                            _prevEntry = nextDefault;
                            _prevRoomName = nextRoomName;
                            _prevRoomPrefix = nextPrefix;
                            this.VolumeFader.BeginFade(this.FadeOutDuration, 0.0f);
                        }
                        else
                        {
                            // Keeping same track but possibly at different volume
                            this.VolumeFader.BeginFade(this.FadeOutDuration, nextDefault.Volume);
                        }
                    }
                }
            }
        }
    }

    // Ending room transition
    private void onResume(LocalEventNotifier.Event e)
    {
        ResumeEvent pe = e as ResumeEvent;
        if (pe.PauseGroup == PauseController.PauseGroup.SequencedPause && pe.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            if (_switchingTracks)
            {
                _switchingTracks = false;
                this.AudioSource.Stop();
                if (_prevEntry.Clip != null)
                {
                    this.AudioSource.clip = _prevEntry.Clip;
                    this.AudioSource.volume = _prevEntry.Volume;
                    this.AudioSource.Play();
                }
            }
        }
    }

    private string getRoomPrefix(string roomName)
    {
        int spaceIndex = roomName.IndexOf(StringExtensions.SPACE);
        return (spaceIndex <= 0 ? roomName : roomName.Substring(0, spaceIndex)).ToLower();
    }
}
