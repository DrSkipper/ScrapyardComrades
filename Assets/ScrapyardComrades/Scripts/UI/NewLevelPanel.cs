using UnityEngine;
using UnityEngine.UI;

public class NewLevelPanel : VoBehavior
{
    public InputField LevelNameField;
    public CyclePanel PlatformsPanel;
    public CyclePanel BackgroundPanel;
    public CyclePanel BgParallaxPanel;
    public RectTransform Selector;
    public TilesetCollection TilesetCollection;
    public OnLevelCreationComplete CompletionCallback;

    public delegate void OnLevelCreationComplete(string levelName, string platformsTileset, string backgroundTileset, string bgParallaxTileset);

    void Awake()
    {
        this.PlatformsPanel.Text.text = this.TilesetCollection.Tilesets[1].name;
        this.BackgroundPanel.Text.text = this.TilesetCollection.Tilesets[0].name;
        this.BgParallaxPanel.Text.text = NONE;
        this.LevelNameField.text = DEFAULT_LEVEL_NAME;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        selectNameEntry();
    }

    void Update()
    {
        if (MenuInput.Start && !_nameEntryEnabled)
        {
            string levelName = this.LevelNameField.text;
            string platforms = this.TilesetCollection.Tilesets[_selectedPlatforms].name;
            string background = this.TilesetCollection.Tilesets[_selectedBackground].name;
            string bgParallax = _selectedBgParallax > -1 ? this.TilesetCollection.Tilesets[_selectedBgParallax].name : null;
            this.LevelNameField.interactable = false;
            this.LevelNameField.DeactivateInputField();
            this.CompletionCallback(levelName, platforms, background, bgParallax);
            this.gameObject.SetActive(false);
        }
        else if (MenuInput.Confirm && _selection == NAME_ENTRY)
        {
            if (_nameEntryEnabled)
            {
                _nameEntryEnabled = false;
                this.LevelNameField.interactable = false;
                this.LevelNameField.DeactivateInputField();
            }
            else
            {
                _nameEntryEnabled = true;
                this.LevelNameField.interactable = true;
                this.LevelNameField.ActivateInputField();
            }
        }
        else if (!_nameEntryEnabled && (MenuInput.NavDown || MenuInput.NavUp))
        {
            
            if (MenuInput.NavDown)
            {
                ++_selection;
                if (_selection > BG_PARALLAX)
                    _selection = NAME_ENTRY;
            }
            else
            {
                --_selection;
                if (_selection < NAME_ENTRY)
                    _selection = BG_PARALLAX;
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
                case BG_PARALLAX:
                    selectBgParallax();
                    break;
            }
        }
        else if (MenuInput.NavLeft)
        {
            if (_selection == PLATFORMS)
            {
                _selectedPlatforms = _selectedPlatforms <= 0 ? this.TilesetCollection.Tilesets.Length - 1 : _selectedPlatforms - 1;
                this.PlatformsPanel.Text.text = this.TilesetCollection.Tilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleLeft();
            }
            else if (_selection == BACKGROUND)
            {
                _selectedBackground = _selectedBackground <= 0 ? this.TilesetCollection.Tilesets.Length - 1 : _selectedBackground - 1;
                this.BackgroundPanel.Text.text = this.TilesetCollection.Tilesets[_selectedBackground].name;
                this.BackgroundPanel.CycleLeft();
            }
            else if (_selection == BG_PARALLAX)
            {
                _selectedBgParallax = _selectedBgParallax <= -1 ? this.TilesetCollection.Tilesets.Length - 1 : _selectedBgParallax - 1;
                this.BgParallaxPanel.Text.text = _selectedBgParallax == -1 ? NONE : this.TilesetCollection.Tilesets[_selectedBgParallax].name;
                this.BgParallaxPanel.CycleLeft();
            }
        }
        else if (MenuInput.NavRight)
        {
            if (_selection == PLATFORMS)
            {
                _selectedPlatforms = _selectedPlatforms >= this.TilesetCollection.Tilesets.Length - 1 ? 0 : _selectedPlatforms + 1;
                this.PlatformsPanel.Text.text = this.TilesetCollection.Tilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleRight();
            }
            else if (_selection == BACKGROUND)
            {
                _selectedBackground = _selectedBackground >= this.TilesetCollection.Tilesets.Length - 1 ? 0 : _selectedBackground + 1;
                this.BackgroundPanel.Text.text = this.TilesetCollection.Tilesets[_selectedBackground].name;
                this.BackgroundPanel.CycleRight();
            }
            else if (_selection == BG_PARALLAX)
            {
                _selectedBgParallax = _selectedBgParallax >= this.TilesetCollection.Tilesets.Length - 1 ? -1 : _selectedBgParallax + 1;
                this.BgParallaxPanel.Text.text = _selectedBgParallax == -1 ? NONE : this.TilesetCollection.Tilesets[_selectedBgParallax].name;
                this.BgParallaxPanel.CycleRight();
            }
        }
    }

    /**
     * Private
     */
    private int _selection;
    private int _selectedPlatforms;
    private int _selectedBackground;
    private int _selectedBgParallax;
    private bool _nameEntryEnabled;
    private const int NAME_ENTRY = 0;
    private const int PLATFORMS = 1;
    private const int BACKGROUND = 2;
    private const int BG_PARALLAX = 3;
    private const string DEFAULT_LEVEL_NAME = "new_level";
    private const string NONE = "NONE";

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

    private void selectBgParallax()
    {
        _selection = BG_PARALLAX;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.BgParallaxPanel.transform).anchoredPosition.y);
    }
}
