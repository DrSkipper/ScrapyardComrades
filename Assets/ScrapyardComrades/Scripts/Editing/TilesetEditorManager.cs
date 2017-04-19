﻿using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[ExecuteInEditMode]
public class TilesetEditorManager : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;
    public RectTransform Cursor;
    public TilesetData TilesetToEdit;
    public float PixelsToUnits;
    public TilesetData CurrentEditingTileset { get; set; }
    public Texture2D Texture { get { return _texture; } }
    [Header("TILE DATA")]
    public Sprite SelectedSprite; // Exposed for debugging
    public TilesetData.SpriteData SelectedSpriteData { get; set; }
    //public SpriteRenderer CursorRenderer { get { if (_cursorSpriteRenderer == null) _cursorSpriteRenderer = this.Cursor.GetComponent<SpriteRenderer>(); return _cursorSpriteRenderer; } }

    void Start()
    {
        Reload();
    }

    public void Reload()
    {
#if UNITY_EDITOR
        if (this.TilesetToEdit == null)
        {
            Debug.LogWarning("Attempted to load null tileset");
            return;
        }

        string tilesetPath = PackedSpriteGroup.INDEXED_TEXTURES_PATH + TilesetData.TILESETS_PATH + this.TilesetToEdit.AtlasName + PackedSpriteGroup.TEXTURE_SUFFIX;
        Debug.Log("loading tileset " + tilesetPath);
        _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(tilesetPath);
        if (_texture == null)
        {
            Debug.LogWarning("Could not find tileset named " + this.TilesetToEdit.AtlasName);
            return;
        }

        _sprites = GetSpritesArrayEditor(tilesetPath);
        _spriteData = this.TilesetToEdit.GetSpriteDataDictionary();
        this.SelectedSprite = null;

        for (int i = 0; i < _sprites.Length; ++i)
        {
            if (!_spriteData.ContainsKey(_sprites[i].name))
                _spriteData.Add(_sprites[i].name, new TilesetData.SpriteData(_sprites[i].name));
        }

        this.TilesetToEdit.ApplySpriteDataDictionary(_spriteData);
        this.MeshRenderer.sharedMaterial.mainTexture = _texture;
#else
        Debug.Log("TilesetEditorManager should not be used at runtime!");
#endif
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
                Vector3 meshSize = this.MeshFilter.sharedMesh.bounds.size;
                this.Cursor.SetPosition2D((sprite.rect.center.x / _texture.width - 0.5f) * meshSize.x, (sprite.rect.center.y / _texture.height - 0.5f) * meshSize.z);
                this.Cursor.sizeDelta = new Vector2(sprite.rect.size.x / _texture.width * meshSize.x, sprite.rect.size.y / _texture.height * meshSize.z);
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

    public static Sprite[] GetSpritesArrayEditor(string path)
    {
#if UNITY_EDITOR
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        //List<Sprite> sprites = new List<Sprite>();
        return sprites.ToArray();
#else
        return null;
#endif
    }

    /**
     * Private
     */
    private Texture2D _texture;
    private Sprite[] _sprites;
    private Dictionary<string, TilesetData.SpriteData> _spriteData;
    //private SpriteRenderer _cursorSpriteRenderer;
}
