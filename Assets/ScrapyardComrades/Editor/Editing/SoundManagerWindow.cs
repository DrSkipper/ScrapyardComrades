using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

/**
 * SoundManagerWindow:
 * Allows the user to set audio clips for each sound key in the game
 */
public class SoundManagerWindow : EditorWindow
{
    public const string SOUND_DATA_PATH = "SoundData.json";

    public SoundData Data;
    public AudioSource AudioSourcePrefab;

    [MenuItem("Window/Sound Manager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SoundManagerWindow), false, "Sound Manager");
    }

    void Awake()
    {
        reload();
        //createFoldoutStyle();
    }

    void OnGUI()
    {
        if (this.Data == null || _soundKeys == null || this.Data.EntriesByEnumIndex == null || this.Data.EntriesByEnumIndex.Count <= (int)SoundData.Key.TOTAL || this.Data.ClipsByEnumIndex == null || this.Data.ClipsByEnumIndex.Count <= (int)SoundData.Key.TOTAL || this.Data.VolumeByEnumIndex == null || this.Data.VolumeByEnumIndex.Count <= (int)SoundData.Key.TOTAL || this.Data.PitchByEnumIndex == null || this.Data.PitchByEnumIndex.Count <= (int)SoundData.Key.TOTAL || this.Data.CooldownsByEnumIndex == null || this.Data.CooldownsByEnumIndex.Count <= (int)SoundData.Key.TOTAL || _foldouts == null || _foldouts.Count <= (int)SoundData.Key.TOTAL)
        {
            reload();
        }

        if (_foldoutStyle == null)
        {
            createFoldoutStyle();
        }

        EditorGUILayout.Separator();
        _filterText = EditorGUILayout.TextField("Filter:", _filterText);
        string filter = _filterText;
        if (!StringExtensions.IsEmpty(filter))
            filter = filter.ToLower();

        _filterClip = (AudioClip)EditorGUILayout.ObjectField("Filter by clip:", _filterClip, typeof(AudioClip), false);
        _unSetFilter = EditorGUILayout.Toggle("Only Show Missing Clips", _unSetFilter);

        if (GUILayout.Button("Remove Empty Entries"))
        {
            for (int i = 0; i < this.Data.EntriesByEnumIndex.Count; ++i)
            {
                SoundData.EntryList entryList = this.Data.EntriesByEnumIndex[i];
                if (entryList != null)
                    entryList.RemoveEmpties();
            }
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, true);
        bool changed = false;
        
        for (int i = 1; i < _soundKeys.Length - 1; ++i)
        {
            SoundData.Key key = (SoundData.Key)_soundKeys.GetValue(i);
            string name = System.Enum.GetName(typeof(SoundData.Key), key);
            
            if (!StringExtensions.IsEmpty(filter) && !name.ToLower().Contains(filter))
                continue;

            if (_unSetFilter && this.Data.ClipsByEnumIndex[(int)key] != null)
                continue;

            if (_filterClip != null && this.Data.ClipsByEnumIndex[(int)key] != _filterClip)
                continue;

            _foldouts[(int)key] = EditorGUILayout.Foldout(_foldouts[(int)key], name, true, _foldoutStyle);

            if (_foldouts[(int)key])
            {
                EditorGUI.BeginChangeCheck();
                ++EditorGUI.indentLevel;
                this.Data.ClipsByEnumIndex[(int)key] = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", this.Data.ClipsByEnumIndex[(int)key], typeof(AudioClip), false);
                this.Data.VolumeByEnumIndex[(int)key] = EditorGUILayout.Slider("Volume", this.Data.VolumeByEnumIndex[(int)key], 0.0f, 1.0f);
                this.Data.PitchByEnumIndex[(int)key] = EditorGUILayout.Slider("Pitch", this.Data.PitchByEnumIndex[(int)key], -3.0f, 3.0f);

                //this.Data.EntriesByEnumIndex[(int)key] = (List<SoundData.Entry>)EditorGUILayout.ObjectField(label: "Entries", obj: this.Data.EntriesByEnumIndex[(int)key], objType: typeof(List<SoundData.Entry>), allowSceneObjects: false);

                if (this.Data.EntriesByEnumIndex[(int)key] != null)
                {
                    for (int j = 0; j < this.Data.EntriesByEnumIndex[(int)key].Count;)
                    {
                        SoundData.Entry entry = this.Data.EntriesByEnumIndex[(int)key].Entries[j];
                        GUILayout.BeginHorizontal();
                        //GUILayout.FlexibleSpace();
                        //GUILayout.Space(30.0f);
                        //bool remove = GUILayout.Button("-");
                        //GUILayout.FlexibleSpace();
                        //GUILayout.Space(1.0f);
                        EditorGUILayout.LabelField("Entry " + j, GUILayout.Width(70.0f));
                        GUILayout.Space(1.0f);
                        bool remove = GUILayout.Button("-");
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        ++EditorGUI.indentLevel;

                        //GUI.Button(new Rect(0, 0, 20, 20), "-");
                        //GUI.Label(new Rect())

                        /*GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        bool remove = GUILayout.Button("-");
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();*/
                        if (remove)
                        {
                            this.Data.EntriesByEnumIndex[(int)key].RemoveAt(j);
                        }
                        else
                        {
                            entry.Clip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", entry.Clip, typeof(AudioClip), false);
                            entry.Volume = EditorGUILayout.Slider("Volume", entry.Volume, 0.0f, 1.0f);
                            entry.Pitch = EditorGUILayout.Slider("Pitch", entry.Pitch, -3.0f, 3.0f);
                            this.Data.EntriesByEnumIndex[(int)key].Entries[j] = entry;
                            ++j;
                        }

                        --EditorGUI.indentLevel;
                    }
                }

                GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();
                GUILayout.Space(35.0f);
                if (GUILayout.Button("Add Entry"))
                {
                    SoundData.Entry entry = new SoundData.Entry();
                    entry.Clip = null;
                    entry.Volume = 1.0f;
                    entry.Pitch = 1.0f;
                    if (this.Data.EntriesByEnumIndex[(int)key] == null)
                        this.Data.EntriesByEnumIndex[(int)key] = new SoundData.EntryList();
                    this.Data.EntriesByEnumIndex[(int)key].Add(entry);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                this.Data.CooldownsByEnumIndex[(int)key] = EditorGUILayout.IntSlider("Cooldown", this.Data.CooldownsByEnumIndex[(int)key], 0, 20);
                changed |= EditorGUI.EndChangeCheck();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Test", GUILayout.Width(200.0f)))
                {
                    if (this.Data.EntriesByEnumIndex[(int)key] != null)
                        PlaySfx(this.Data.EntriesByEnumIndex[(int)key]);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }
        }

        if (changed)
            EditorUtility.SetDirty(this.Data);

        EditorGUILayout.EndScrollView();
    }

    public void PlaySfx(SoundData.EntryList entries)
    {
        if (_sources == null)
            _sources = new List<AudioSource>();

        for (int i = _sources.Count - 1; i >= 0; --i)
        {
            if (_sources[i] == null)
                _sources.RemoveAt(i);
            else
                _sources[i].Stop();
        }

        while (_sources.Count < entries.Count)
        {
            AudioSource source = Instantiate<AudioSource>(this.AudioSourcePrefab);
            source.gameObject.hideFlags = HideFlags.DontSaveInEditor;
            _sources.Add(source);
        }

        for (int i = 0; i < entries.Count; ++i)
        {
            AudioSource source = _sources[i];
            source.clip = entries.Entries[i].Clip;
            source.volume = entries.Entries[i].Volume;
            source.pitch = entries.Entries[i].Pitch;
            source.Play();
        }
    }


    /**
     * Private
     */
    private System.Array _soundKeys;
    private Vector2 _scrollPos = Vector2.zero;
    private List<bool> _foldouts;
    private GUIStyle _foldoutStyle;
    private string _filterText;
    private bool _unSetFilter;
    private AudioClip _filterClip;
    private List<AudioSource> _sources;

    private void createFoldoutStyle()
    {
        _foldoutStyle = new GUIStyle(EditorStyles.foldout);
        _foldoutStyle.fontStyle = FontStyle.Bold;
    }

    private void reload()
    {
        if (this.Data == null)
        {
            Debug.LogWarning("Null Sound Data in Sound Manager Window, close the window, then set the SoundData as the default property on the SoundManagerWindow script, then re-open the window");
            this.Close();
            return;
        }

        _soundKeys = System.Enum.GetValues(typeof(SoundData.Key));
        popuplateClips();
        populateVolumes();
        populatePitches();
        populateEntries();
        populateCooldowns();
        populateFoldouts();
    }

    private void popuplateClips()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (this.Data.ClipsByEnumIndex != null)
        {
            // Too big
            if (this.Data.ClipsByEnumIndex.Count > maxIndex + 1)
            {
                this.Data.ClipsByEnumIndex.RemoveRange(maxIndex, this.Data.ClipsByEnumIndex.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (this.Data.ClipsByEnumIndex.Count == maxIndex + 1)
            {
                return;
            }
        }

        List<AudioClip> clips = new List<AudioClip>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (this.Data.ClipsByEnumIndex != null && this.Data.ClipsByEnumIndex.Count > 0)
        {
            clips.AddRange(this.Data.ClipsByEnumIndex);
        }

        for (int i = clips.Count; i <= maxIndex; ++i)
        {
            clips.Add(null);
        }

        this.Data.ClipsByEnumIndex = clips;
    }

    private void populateVolumes()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (this.Data.VolumeByEnumIndex != null)
        {
            // Too big
            if (this.Data.VolumeByEnumIndex.Count > maxIndex + 1)
            {
                this.Data.VolumeByEnumIndex.RemoveRange(maxIndex, this.Data.VolumeByEnumIndex.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (this.Data.VolumeByEnumIndex.Count == maxIndex + 1)
            {
                return;
            }
        }

        List<float> volumes = new List<float>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (this.Data.VolumeByEnumIndex != null && this.Data.VolumeByEnumIndex.Count > 0)
        {
            volumes.AddRange(this.Data.VolumeByEnumIndex);
        }

        for (int i = volumes.Count; i <= maxIndex; ++i)
        {
            volumes.Add(1.0f);
        }

        this.Data.VolumeByEnumIndex = volumes;
    }

    private void populatePitches()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (this.Data.PitchByEnumIndex != null)
        {
            // Too big
            if (this.Data.PitchByEnumIndex.Count > maxIndex + 1)
            {
                this.Data.PitchByEnumIndex.RemoveRange(maxIndex, this.Data.PitchByEnumIndex.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (this.Data.PitchByEnumIndex.Count == maxIndex + 1)
            {
                return;
            }
        }

        List<float> pitches = new List<float>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (this.Data.PitchByEnumIndex != null && this.Data.PitchByEnumIndex.Count > 0)
        {
            pitches.AddRange(this.Data.PitchByEnumIndex);
        }

        for (int i = pitches.Count; i <= maxIndex; ++i)
        {
            pitches.Add(1.0f);
        }

        this.Data.PitchByEnumIndex = pitches;
    }

    private void populateEntries()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (this.Data.EntriesByEnumIndex != null)
        {
            // Too big
            if (this.Data.EntriesByEnumIndex.Count > maxIndex + 1)
            {
                this.Data.EntriesByEnumIndex.RemoveRange(maxIndex, this.Data.EntriesByEnumIndex.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (this.Data.EntriesByEnumIndex.Count == maxIndex + 1)
            {
                // Temp code to eliminate empties
                /*for (int i = 0; i < this.Data.EntriesByEnumIndex.Count; ++i)
                {
                    List<SoundData.Entry> entryList = this.Data.EntriesByEnumIndex[i];
                    if (entryList != null)
                    {
                        for (int j = entryList.Count - 1; j >= 0; --j)
                        {
                            if (entryList[j].Clip == null)
                                entryList.RemoveAt(j);
                        }
                    }
                }*/

                return;
            }
        }

        List<SoundData.EntryList> entries = new List<SoundData.EntryList>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (this.Data.EntriesByEnumIndex != null && this.Data.EntriesByEnumIndex.Count > 0)
        {
            entries.AddRange(this.Data.EntriesByEnumIndex);
        }

        for (int i = entries.Count; i <= maxIndex; ++i)
        {
            SoundData.EntryList entryList = new SoundData.EntryList();
            // Temp code to add from previous system
            if (this.Data.ClipsByEnumIndex != null && this.Data.ClipsByEnumIndex.Count > i && this.Data.ClipsByEnumIndex[i] != null)
            {
                SoundData.Entry entry = new SoundData.Entry();
                entry.Clip = this.Data.ClipsByEnumIndex[i];
                entry.Volume = this.Data.VolumeByEnumIndex[i];
                entry.Pitch = this.Data.PitchByEnumIndex[i];
                entryList.Add(entry);
            }
            entries.Add(entryList);
        }

        this.Data.EntriesByEnumIndex = entries;
    }

    private void populateFoldouts()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (_foldouts != null)
        {
            // Too big
            if (_foldouts.Count > maxIndex + 1)
            {
                _foldouts.RemoveRange(maxIndex, _foldouts.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (_foldouts.Count == maxIndex + 1)
            {
                return;
            }
        }

        List<bool> foldouts = new List<bool>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (_foldouts != null && _foldouts.Count > 0)
        {
            foldouts.AddRange(_foldouts);
        }

        for (int i = foldouts.Count; i <= maxIndex; ++i)
        {
            foldouts.Add(false);
        }

        _foldouts = foldouts;
    }

    private void populateCooldowns()
    {
        int maxIndex = (int)SoundData.Key.TOTAL;

        if (this.Data.CooldownsByEnumIndex != null)
        {
            // Too big
            if (this.Data.CooldownsByEnumIndex.Count > maxIndex + 1)
            {
                this.Data.CooldownsByEnumIndex.RemoveRange(maxIndex, this.Data.CooldownsByEnumIndex.Count - maxIndex);
                return;
            }

            // Perfect already
            else if (this.Data.CooldownsByEnumIndex.Count == maxIndex + 1)
            {
                return;
            }
        }

        List<int> cooldowns = new List<int>((int)SoundData.Key.TOTAL + 1);

        // Too small
        if (this.Data.CooldownsByEnumIndex != null && this.Data.CooldownsByEnumIndex.Count > 0)
        {
            cooldowns.AddRange(this.Data.CooldownsByEnumIndex);
        }

        for (int i = cooldowns.Count; i <= maxIndex; ++i)
        {
            cooldowns.Add(2);
        }

        this.Data.CooldownsByEnumIndex = cooldowns;
    }
}
