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
    public MapEditorManager Manager;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorTilesLayer;
        Texture2D atlas = Manager.GetAtlasForName(_layer.Tileset.AtlasName);
        this.Grid.InitializeGridForSize(atlas.width / this.Grid.GridSpaceSize, atlas.height / this.Grid.GridSpaceSize);
        this.AtlasBackdrop.sizeDelta = new Vector2(atlas.width, atlas.height);

        _sprites = atlas.GetSprites();

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

        foreach (Sprite sprite in _sprites.Values)
        {
            PooledObject tileSpriteObject = this.TileSpritePrefab.Retain();
            tileSpriteObject.GetComponent<Image>().sprite = sprite;
            tileSpriteObject.transform.SetParent(this.AtlasBackdrop, false);
            ((RectTransform)tileSpriteObject.transform).anchoredPosition = new Vector2(sprite.rect.x, sprite.rect.y);
            tileSpriteObject.transform.localScale = new Vector3(1, 1, 1);
            tileSpriteObject.transform.SetLocalZ(0);
            tileSpriteObject.transform.SetAsFirstSibling();
            _tileSpriteObjects.Add(tileSpriteObject);
        }

        _gridPos = _sprites.ContainsKey(_layer.CurrentSpriteName) ? (IntegerVector)_sprites[_layer.CurrentSpriteName].rect.min / this.Grid.GridSpaceSize: IntegerVector.Zero;
        updateVisual();
    }

    void Update()
    {
        if (MapEditorInput.Action)
        {
            _layer.AutoTileEnabled = !_layer.AutoTileEnabled;
            updateVisual();
        }
        else if (MapEditorInput.NavLeft)
        {
            _gridPos = this.Grid.MoveLeft(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MapEditorInput.NavRight)
        {
            _gridPos = this.Grid.MoveRight(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MapEditorInput.NavDown)
        {
            _gridPos = this.Grid.MoveDown(_gridPos);
            updateSelection();
            updateVisual();
        }
        else if (MapEditorInput.NavUp)
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
            if (Mathf.RoundToInt(sprite.rect.x) == _gridPos.X * this.Grid.GridSpaceSize && Mathf.RoundToInt(sprite.rect.y) == _gridPos.Y * this.Grid.GridSpaceSize)
                return sprite;
        }
        return null;
    }
}
