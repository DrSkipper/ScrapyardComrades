using UnityEngine;
using UnityEngine.UI;

public class NewLevelPanel : VoBehavior
{
    public InputField LevelNameField;
    public CyclePanel PlatformsPanel;
    public CyclePanel BackgroundPanel;
    public RectTransform Selector;
    public TilesetData[] PlatformTilesets;
    public TilesetData[] BackgroundTilesets;
    public OnLevelCreationComplete CompletionCallback;

    public delegate void OnLevelCreationComplete(string levelName, string platformsTileset, string backgroundTileset);

    void Awake()
    {
        this.PlatformsPanel.Text.text = this.PlatformTilesets[0].name;
        this.BackgroundPanel.Text.text = this.BackgroundTilesets[0].name;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        selectNameEntry();
    }

    void Update()
    {
        if (MapEditorInput.Start)
        {
            string levelName = this.LevelNameField.text;
            string platforms = this.PlatformTilesets[_selectedPlatforms].name;
            string background = this.BackgroundTilesets[_selectedBackground].name;
            this.LevelNameField.DeactivateInputField();
            this.CompletionCallback(levelName, platforms, background);
            this.gameObject.SetActive(false);
        }
        else if (MapEditorInput.Confirm && _selection == NAME_ENTRY)
        {
            if (_nameEntryEnabled)
            {
                _nameEntryEnabled = false;
                this.LevelNameField.DeactivateInputField();
            }
            else
            {
                _nameEntryEnabled = true;
                this.LevelNameField.ActivateInputField();
            }
        }
        else if (!_nameEntryEnabled && (MapEditorInput.NavDown || MapEditorInput.NavUp))
        {
            
            if (MapEditorInput.NavDown)
            {
                ++_selection;
                if (_selection > BACKGROUND)
                    _selection = NAME_ENTRY;
            }
            else
            {
                --_selection;
                if (_selection < NAME_ENTRY)
                    _selection = BACKGROUND;
            }

            switch (_selection)
            {
                default:
                case NAME_ENTRY:
                    selectNameEntry();
                    break;
                case PLATFORMS:
                    selectPlatforms();
                    break;
                case BACKGROUND:
                    selectBackground();
                    break;
            }
        }
        else if (MapEditorInput.NavLeft)
        {
            if (_selection == PLATFORMS)
            {
                _selectedPlatforms = _selectedPlatforms <= 0 ? this.PlatformTilesets.Length - 1 : _selectedPlatforms - 1;
                this.PlatformsPanel.Text.text = this.PlatformTilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleLeft();
            }
            else if (_selection == BACKGROUND)
            {
                _selectedBackground = _selectedBackground <= 0 ? this.BackgroundTilesets.Length - 1 : _selectedBackground - 1;
                this.BackgroundPanel.Text.text = this.BackgroundTilesets[_selectedBackground].name;
                this.BackgroundPanel.CycleLeft();
            }
        }
        else if (MapEditorInput.NavRight)
        {
            if (_selection == PLATFORMS)
            {
                _selectedPlatforms = _selectedPlatforms >= this.PlatformTilesets.Length - 1 ? 0 : _selectedPlatforms + 1;
                this.PlatformsPanel.Text.text = this.PlatformTilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleRight();
            }
            else if (_selection == BACKGROUND)
            {
                _selectedBackground = _selectedBackground >= this.BackgroundTilesets.Length - 1 ? 0 : _selectedBackground + 1;
                this.BackgroundPanel.Text.text = this.BackgroundTilesets[_selectedBackground].name;
                this.BackgroundPanel.CycleRight();
            }
        }
    }

    /**
     * Private
     */
    private int _selection;
    private int _selectedPlatforms;
    private int _selectedBackground;
    private bool _nameEntryEnabled;
    private const int NAME_ENTRY = 0;
    private const int PLATFORMS = 1;
    private const int BACKGROUND = 2;

    private void selectNameEntry()
    {
        _selection = NAME_ENTRY;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.LevelNameField.transform).anchoredPosition.y);
    }

    private void selectPlatforms()
    {
        _selection = PLATFORMS;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.PlatformsPanel.transform).anchoredPosition.y);
    }

    private void selectBackground()
    {
        _selection = BACKGROUND;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.BackgroundPanel.transform).anchoredPosition.y);
    }
}
