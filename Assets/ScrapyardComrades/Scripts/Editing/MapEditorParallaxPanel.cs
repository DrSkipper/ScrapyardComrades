using UnityEngine;
using UnityEngine.UI;

public class MapEditorParallaxPanel : MonoBehaviour
{
    [HideInInspector]
    public Sprite[] ValidSprites;
    public Image SpriteImage;

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
