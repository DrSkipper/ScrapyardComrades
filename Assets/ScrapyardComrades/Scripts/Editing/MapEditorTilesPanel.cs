using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapEditorTilesPanel : MonoBehaviour
{
    public PooledObject TileSpritePrefab;
    public RectTransform AtlasBackdrop;
    public RectTransform Cursor;
    public GameObject AutotileValue;
    public Image SelectionImage;
    public MapEditorGrid Grid;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorTilesLayer;
        _sprites = Texture2DExtensions.GetSprites(TilesetData.TILESETS_PATH, _layer.Tileset.AtlasName);

        // If spritesheet has tiles offset by non-tile-size, find the offset for our grid
        foreach (Sprite sprite in _sprites.Values)
        {
            int x = Mathf.RoundToInt(sprite.rect.x);
            int y = Mathf.RoundToInt(sprite.rect.y);
            int xMod = x % this.Grid.GridSpaceSize;
            int yMod = y % this.Grid.GridSpaceSize;

            _gridOffset.X = x == 0 || xMod == 0 ? 0 : this.Grid.GridSpaceSize - xMod;
            _gridOffset.Y = y == 0 || yMod == 0 ? 0 : this.Grid.GridSpaceSize - yMod;
            break;
        }

        if (_tileSpriteObjects != null)
        {
            for (int i = 0; i < _tileSpriteObjects.Count; ++i)
            {
                _tileSpriteObjects[i].transform.SetParent(null);
                _tileSpriteObjects[i].Release();
            }
            _tileSpriteObjects.Clear();
        }
        else
        {
            _tileSpriteObjects = new List<PooledObject>();
        }
        
        IntegerVector max = IntegerVector.Zero;

        foreach (Sprite sprite in _sprites.Values)
        {
            PooledObject tileSpriteObject = this.TileSpritePrefab.Retain();
            tileSpriteObject.GetComponent<Image>().sprite = sprite;
            tileSpriteObject.transform.SetParent(this.AtlasBackdrop, false);
            ((RectTransform)tileSpriteObject.transform).anchoredPosition = new Vector2(sprite.rect.x + _gridOffset.X, sprite.rect.y + _gridOffset.Y);
            if (sprite.rect.x + _gridOffset.X + this.Grid.GridSpaceSize > max.X)
                max.X = Mathf.RoundToInt(sprite.rect.x) + _gridOffset.X + this.Grid.GridSpaceSize;
            if (sprite.rect.y + _gridOffset.Y + this.Grid.GridSpaceSize > max.Y)
                max.Y = Mathf.RoundToInt(sprite.rect.y) + _gridOffset.Y + this.Grid.GridSpaceSize;
            tileSpriteObject.transform.localScale = new Vector3(1, 1, 1);
            tileSpriteObject.transform.SetLocalZ(0);
            tileSpriteObject.transform.SetAsFirstSibling();
            _tileSpriteObjects.Add(tileSpriteObject);
        }

        this.Grid.InitializeGridForSize(max.X / this.Grid.GridSpaceSize, max.Y / this.Grid.GridSpaceSize);
        this.AtlasBackdrop.sizeDelta = new Vector2(max.X, max.Y);

        _gridPos = _sprites.ContainsKey(_layer.CurrentSpriteName) ? ((IntegerVector)_sprites[_layer.CurrentSpriteName].rect.min + _gridOffset) / this.Grid.GridSpaceSize: IntegerVector.Zero;
        updateVisual();
    }

    void Update()
    {
        if (MenuInput.Action)
        {
            _layer.AutoTileEnabled = !_layer.AutoTileEnabled;
            updateVisual();
        }
        else if (MenuInput.NavLeft)
        {
            _gridPos = this.Grid.MoveLeft(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MenuInput.NavRight)
        {
            _gridPos = this.Grid.MoveRight(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MenuInput.NavDown)
        {
            _gridPos = this.Grid.MoveDown(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MenuInput.NavUp)
        {
            _gridPos = this.Grid.MoveUp(_gridPos);
            updateSelection();
            updateVisual();
        }
    }

    /**
     * Private
     */
    private MapEditorTilesLayer _layer;
    private Dictionary<string, Sprite> _sprites;
    private List<PooledObject> _tileSpriteObjects;
    private IntegerVector _gridPos;
    private IntegerVector _gridOffset = IntegerVector.Zero;

    private void updateSelection()
    {
        Sprite selectedSprite = findSpriteAtCursor();
        if (selectedSprite != null && !_layer.SpriteDataDict.ContainsKey(selectedSprite.name))
            selectedSprite = null;
        _layer.CurrentSpriteName = selectedSprite == null ? NewMapInfo.MapTile.EMPTY_TILE_SPRITE_NAME : selectedSprite.name;
    }

    private void updateVisual()
    {
        this.AutotileValue.SetActive(_layer.AutoTileEnabled);
        this.SelectionImage.sprite = _sprites.ContainsKey(_layer.CurrentSpriteName) ? _sprites[_layer.CurrentSpriteName] : null;
        this.Cursor.anchoredPosition = this.Grid.GridToWorld(_gridPos);
    }

    private Sprite findSpriteAtCursor()
    {
        foreach (Sprite sprite in _sprites.Values)
        {
            if (Mathf.RoundToInt(sprite.rect.x) + _gridOffset.X == _gridPos.X * this.Grid.GridSpaceSize && Mathf.RoundToInt(sprite.rect.y) + _gridOffset.Y == _gridPos.Y * this.Grid.GridSpaceSize)
                return sprite;
        }
        return null;
    }
}
