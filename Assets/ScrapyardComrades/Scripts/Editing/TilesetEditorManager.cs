using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TilesetEditorManager : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;
    public Transform Cursor;
    public TilesetData TilesetToEdit;
    public float PixelsToUnits;
    public TilesetData CurrentEditingTileset { get; set; }
    public Texture2D Texture { get { return _texture; } }
    [Header("TILE DATA")]
    public Sprite SelectedSprite; // Exposed for debugging
    public TilesetData.SpriteData SelectedSpriteData { get; set; }

    void Start()
    {
        Reload();
    }

    public void Reload()
    {
        if (this.TilesetToEdit == null)
        {
            Debug.LogWarning("Attempted to load null tileset");
            return;
        }

        _texture = Resources.Load<Texture2D>(this.TilesetToEdit.AtlasName);
        if (_texture == null)
        {
            Debug.LogWarning("Could not find tileset named " + this.TilesetToEdit.AtlasName);
            return;
        }

        _sprites = _texture.GetSpritesArray();
        _spriteData = this.TilesetToEdit.GetSpriteDataDictionary();
        this.SelectedSprite = null;

        for (int i = 0; i < _sprites.Length; ++i)
        {
            if (!_spriteData.ContainsKey(_sprites[i].name))
                _spriteData.Add(_sprites[i].name, new TilesetData.SpriteData(_sprites[i].name));
        }

        this.TilesetToEdit.ApplySpriteDataDictionary(_spriteData);
        this.MeshRenderer.sharedMaterial.mainTexture = _texture;
    }

    public void SelectSpriteAtPixel(IntegerVector pixel)
    {
        if (_spriteData == null)
            this.Reload();

        for (int i = 0; i < _sprites.Length; ++i)
        {
            Sprite sprite = _sprites[i];
            if (sprite.GetIntegerBounds().Contains(pixel))
            {
                this.SelectedSprite = sprite;
                this.SelectedSpriteData = _spriteData[sprite.name];
                Vector2 meshSize = this.MeshFilter.mesh.bounds.size;
                this.Cursor.SetPosition2D((sprite.rect.center.x / _texture.width - _texture.width / 2.0f) * meshSize.x, (sprite.rect.center.y / _texture.height - _texture.height / 2.0f) * meshSize.y);
                break;
            }
        }
    }

    public bool ApplySpriteData(TilesetData.TileType tileType, bool autoTile)
    {
        if (this.SelectedSpriteData.Type != tileType || this.SelectedSpriteData.AllowAutotile != autoTile)
        {
            TilesetData.SpriteData spriteData = _spriteData[this.SelectedSprite.name];
            spriteData.Type = tileType;
            spriteData.AllowAutotile = autoTile;
            _spriteData[this.SelectedSprite.name] = spriteData;
            this.SelectedSpriteData = spriteData;
            this.TilesetToEdit.ApplySpriteDataDictionary(_spriteData);
            return true;
        }
        return false;
    }

    /**
     * Private
     */
    private Texture2D _texture;
    private Sprite[] _sprites;
    private Dictionary<string, TilesetData.SpriteData> _spriteData;
}
