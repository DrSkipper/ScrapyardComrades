using UnityEngine;
using UnityEngine.UI;

public class MapEditorParallaxPanel : MonoBehaviour
{
    [HideInInspector]
    public Sprite[] ValidSprites;
    public Image SpriteImage;
    public Text HeightValueText;
    public Text RatioValueText;
    public GameObject LoopValueObject;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorParallaxLayer;
        _currentSpriteIndex = findCurrentSpriteIndex();
        updateVisual();
    }

    void Update()
    {
        if (MapEditorInput.CyclePrev)
        {
            --_currentSpriteIndex;
            if (_currentSpriteIndex < -1)
                _currentSpriteIndex = this.ValidSprites.Length - 1;
            _layer.SpriteName = _currentSpriteIndex == -1 ? null : this.ValidSprites[_currentSpriteIndex].name;
            updateVisual();
        }
        else if (MapEditorInput.CycleNext)
        {
            ++_currentSpriteIndex;
            if (_currentSpriteIndex >= this.ValidSprites.Length)
                _currentSpriteIndex = -1;
            _layer.SpriteName = _currentSpriteIndex == -1 ? null : this.ValidSprites[_currentSpriteIndex].name;
            updateVisual();
        }
        else if (MapEditorInput.Action)
        {
            _layer.Loops = !_layer.Loops;
            updateVisual();
        }
        else if (MapEditorInput.NavDown)
        {
            _layer.Height = Mathf.Max(0.0f, _layer.Height - 0.1f);
            updateVisual();
        }
        else if (MapEditorInput.NavUp)
        {
            _layer.Height = Mathf.Min(1.0f, _layer.Height + 0.1f);
            updateVisual();
        }
        else if (MapEditorInput.NavLeft)
        {
            _layer.ParallaxRatio = Mathf.Max(0.0f, _layer.ParallaxRatio - 0.1f);
            updateVisual();
        }
        else if (MapEditorInput.NavRight)
        {
            _layer.ParallaxRatio = Mathf.Min(1.0f, _layer.ParallaxRatio + 0.1f);
            updateVisual();
        }
    }

    /**
     * Private
     */
    private MapEditorParallaxLayer _layer;
    private int _currentSpriteIndex;

    private void updateVisual()
    {
        if (_currentSpriteIndex == -1)
            this.SpriteImage.sprite = null;
        else
            this.SpriteImage.sprite = this.ValidSprites[_currentSpriteIndex];
        this.LoopValueObject.SetActive(_layer.Loops);
        this.HeightValueText.text = "" + _layer.Height;
        this.RatioValueText.text = "" + _layer.ParallaxRatio;
    }

    private int findCurrentSpriteIndex()
    {
        for (int i = 0; i < this.ValidSprites.Length; ++i)
        {
            if (this.ValidSprites[i].name == _layer.SpriteName)
                return i;
        }
        return -1;
    }
}
