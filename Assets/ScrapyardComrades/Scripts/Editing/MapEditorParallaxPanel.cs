using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapEditorParallaxPanel : MonoBehaviour
{
    [HideInInspector]
    public List<Sprite> ValidSprites;
    public Image SpriteImage;
    public Text HeightValueText;
    public Text RatioValueText;
    public Text XPosValueText;
    public Text LayerNameText;
    public GameObject LoopValueObject;
    public GameObject LitValueObject;
    public string[] ValidLayerNames;

    public const float INCREMENT = 0.05f;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorParallaxLayer;
        _currentSpriteIndex = findCurrentSpriteIndex();
        _currentLayerNameIndex = findCurrentLayerNameIndex();
        updateVisual();
    }

    void Update()
    {
        if (MapEditorInput.CyclePrev)
        {
            --_currentSpriteIndex;
            if (_currentSpriteIndex < -1)
                _currentSpriteIndex = this.ValidSprites.Count - 1;
            _layer.SpriteName = _currentSpriteIndex == -1 ? null : this.ValidSprites[_currentSpriteIndex].name;
            updateVisual();
        }
        else if (MapEditorInput.CycleNext)
        {
            ++_currentSpriteIndex;
            if (_currentSpriteIndex >= this.ValidSprites.Count)
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
            _layer.Height = Mathf.Max(0.0f, _layer.Height - INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavUp)
        {
            _layer.Height = Mathf.Min(1.0f, _layer.Height + INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavLeft)
        {
            _layer.XPosition = Mathf.Max(0.0f, _layer.XPosition - INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.NavRight)
        {
            _layer.XPosition = Mathf.Min(1.0f, _layer.XPosition + INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeLeft || MapEditorInput.ResizeDown)
        {
            _layer.ParallaxRatio = Mathf.Max(0.0f, _layer.ParallaxRatio - INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.ResizeRight || MapEditorInput.ResizeUp)
        {
            _layer.ParallaxRatio = Mathf.Min(1.0f, _layer.ParallaxRatio + INCREMENT);
            updateVisual();
        }
        else if (MapEditorInput.Cancel)
        {
            _currentLayerNameIndex = _currentLayerNameIndex >= ValidLayerNames.Length - 1 ? 0 : _currentLayerNameIndex + 1;
            _layer.LayerName = ValidLayerNames[_currentLayerNameIndex];
            updateVisual();
        }
        else if (MapEditorInput.Confirm)
        {
            _layer.Lit = !_layer.Lit;
            updateVisual();
        }
    }

    /**
     * Private
     */
    private MapEditorParallaxLayer _layer;
    private int _currentSpriteIndex;
    private int _currentLayerNameIndex;

    private void updateVisual()
    {
        if (_currentSpriteIndex == -1)
            this.SpriteImage.sprite = null;
        else
            this.SpriteImage.sprite = this.ValidSprites[_currentSpriteIndex];
        this.LoopValueObject.SetActive(_layer.Loops);
        this.LitValueObject.SetActive(_layer.Lit);
        this.HeightValueText.text = _layer.Height.ToString("0.00");
        this.RatioValueText.text = _layer.ParallaxRatio.ToString("0.00");
        this.XPosValueText.text = _layer.XPosition.ToString("0.00");
        this.LayerNameText.text = _layer.LayerName;
    }

    private int findCurrentSpriteIndex()
    {
        for (int i = 0; i < this.ValidSprites.Count; ++i)
        {
            if (this.ValidSprites[i].name == _layer.SpriteName)
                return i;
        }
        return -1;
    }

    private int findCurrentLayerNameIndex()
    {
        for (int i = 0; i < ValidLayerNames.Length; ++i)
        {
            if (ValidLayerNames[i] == _layer.LayerName)
                return i;
        }
        return 0;
    }
}
