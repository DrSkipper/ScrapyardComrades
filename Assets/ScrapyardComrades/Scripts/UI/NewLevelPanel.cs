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

    public void Show()
    {
        this.gameObject.SetActive(true);
        selectPlatforms();
        this.LevelNameField.ActivateInputField();
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
        else if (MapEditorInput.NavDown || MapEditorInput.NavUp)
        {
            if (_platformsSelected)
                selectBackground();
            else
                selectPlatforms();
        }
        else if (MapEditorInput.NavLeft)
        {
            if (_platformsSelected)
            {
                _selectedPlatforms = _selectedPlatforms <= 0 ? this.PlatformTilesets.Length - 1 : _selectedPlatforms - 1;
                this.PlatformsPanel.Text.text = this.PlatformTilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleLeft();
            }
            else
            {
                _selectedBackground = _selectedBackground <= 0 ? this.BackgroundTilesets.Length - 1 : _selectedBackground - 1;
                this.BackgroundPanel.Text.text = this.BackgroundTilesets[_selectedBackground].name;
                this.BackgroundPanel.CycleLeft();
            }
        }
        else if (MapEditorInput.NavRight)
        {
            if (_platformsSelected)
            {
                _selectedPlatforms = _selectedPlatforms >= this.PlatformTilesets.Length - 1 ? 0 : _selectedPlatforms + 1;
                this.PlatformsPanel.Text.text = this.PlatformTilesets[_selectedPlatforms].name;
                this.PlatformsPanel.CycleRight();
            }
            else
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
    private bool _platformsSelected;
    private int _selectedPlatforms;
    private int _selectedBackground;

    private void selectPlatforms()
    {
        _platformsSelected = true;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.PlatformsPanel.transform).anchoredPosition.y);
    }

    private void selectBackground()
    {
        _platformsSelected = false;
        this.Selector.anchoredPosition = new Vector2(this.Selector.anchoredPosition.x, ((RectTransform)this.BackgroundPanel.transform).anchoredPosition.y);
    }
}
