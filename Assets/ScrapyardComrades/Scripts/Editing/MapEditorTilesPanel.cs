using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapEditorTilesPanel : MonoBehaviour
{
    public Image TileSpritePrefab;
    public RectTransform AtlasBackdrop;
    public RectTransform Cursor;
    public GameObject AutotileValue;
    public Image SelectionImage;
    public MapEditorManager Manager;

    public void ShowForLayer(MapEditorLayer layer)
    {
        _layer = layer as MapEditorTilesLayer;
        Texture2D atlas = Manager.GetAtlasForName(_layer.Tileset.AtlasName);
        _sprites = atlas.GetSprites();
        updateVisual();
    }

    /**
     * Private
     */
    private MapEditorTilesLayer _layer;
    private Dictionary<string, Sprite> _sprites;

    private void updateVisual()
    {
        this.AutotileValue.SetActive(_layer.AutoTileEnabled);
        this.SelectionImage.sprite = _sprites.ContainsKey(_layer.CurrentSpriteName) ? _sprites[_layer.CurrentSpriteName] : null;
    }
}
