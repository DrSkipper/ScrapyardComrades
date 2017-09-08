using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveSlotList : MonoBehaviour, IPausable
{
    public RectTransform Cursor;
    public RectTransform ListParent;
    public SaveSlotEntry SaveSlotPrefab;
    public int SaveSlotEntryHeight = 50;
    public string GameplayScene;
    public ConfirmationPanel EraseConfirmationPanel;
    public bool InfiniteRetries = false;

    public const string ERASE_TAG = "erase";

    void Start()
    {
        MenuInput.RewiredPlayer.controllers.maps.SetMapsEnabled(true, "Menu");
        this.EraseConfirmationPanel.ActionCallback += onEraseCallback;
        loadSaveSlotData();
        _current = 0;
        selectCurrent();
    }

    void Update()
    {
        if (MenuInput.Confirm)
        {
            bool loaded = false;
            if (_current < _slotSummaries.Length)
            {
                if (!this.InfiniteRetries && _slotSummaries[_current].UnsafeSave)
                {
                    //TODO: put message on screen that this save cannot be loaded
                }
                else
                {
                    SaveData.LoadFromDisk(_slotSummaries[_current].Name);
                    SaveData.UnsafeSave = true;
                    SaveData.SaveToDisk();
                    loaded = true;
                }
            }
            else
            {
                SaveData.LoadFromDisk(SaveSlotData.CreateNewSlotName(_slotSummaries));
                loaded = true;
            }

            if (loaded)
            {
                if (!StringExtensions.IsEmpty(SaveData.LastSaveRoom))
                {
                    ScenePersistentLoading.BeginLoading(SaveData.LastSaveRoom);
                }

                MenuInput.RewiredPlayer.controllers.maps.SetMapsEnabled(false, "Menu");
                SceneManager.LoadScene(this.GameplayScene, LoadSceneMode.Single);
            }
        }
        else
        {
            bool up = MenuInput.NavUp || MenuInput.ResizeUp;
            bool down = MenuInput.NavDown || MenuInput.ResizeDown;

            if (up || down)
            {
                _current = Mathf.Clamp(_current + (up ? -1 : 1), 0, _entries.Count - 1);
                selectCurrent();
            }
            else if (_current < _slotSummaries.Length && MenuInput.Exit)
            {
                PauseController.UserPause();
            }
        }
    }

    /**
     * Private
     */
    private int _current;
    private List<RectTransform> _entries;
    private SaveSlotData.SlotSummary[] _slotSummaries;

    private void selectCurrent()
    {
        this.Cursor.anchoredPosition = _entries[_current].anchoredPosition;
    }
    
    private void loadSaveSlotData()
    {
        _slotSummaries = SaveSlotData.GetAllSlots();
        _entries = new List<RectTransform>(Mathf.Min(_slotSummaries.Length + 1, SaveSlotData.MAX_SLOTS));

        for (int i = 0; i < _slotSummaries.Length; ++i)
        {
            SaveSlotEntry entry = Instantiate<SaveSlotEntry>(SaveSlotPrefab);
            entry.transform.SetParent(this.ListParent, false);
            (entry.transform as RectTransform).anchoredPosition = new Vector2(0, -i * this.SaveSlotEntryHeight);
            entry.Configure(_slotSummaries[i]);
            _entries.Add(entry.transform as RectTransform);
        }

        if (_entries.Count < SaveSlotData.MAX_SLOTS)
        {
            SaveSlotEntry entry = Instantiate<SaveSlotEntry>(SaveSlotPrefab);
            entry.transform.SetParent(this.ListParent, false);
            (entry.transform as RectTransform).anchoredPosition = new Vector2(0, -_entries.Count * this.SaveSlotEntryHeight);
            entry.ConfigureForEmpty();
            _entries.Add(entry.transform as RectTransform);
        }

        this.Cursor.transform.SetAsLastSibling();
    }

    private void onEraseCallback(bool confirmed)
    {
        if (confirmed)
        {
            SaveData.EraseSaveSlot(_slotSummaries[_current].Name);
            for (int i = 0; i < _entries.Count; ++i)
            {
                _entries[i].SetParent(null);
            }
            _entries.Clear();
            loadSaveSlotData();
            if (_current >= _entries.Count)
                _current = _entries.Count - 1;
            selectCurrent();
        }

        PauseController.UserResume();
    }
}
