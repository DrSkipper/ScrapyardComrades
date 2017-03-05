using UnityEngine;
using System.Collections.Generic;

public class MapEditorManager : MonoBehaviour
{
    public struct Brush
    {
        public int Layer;
        public int SpriteIndex; //TODO: Handle copy-paste brush (+ fill option?)
    }

    public MapEditorQuad Quad;
    public MapEditorGrid Grid;
    public MapEditorCursor Cursor;
    public Texture2D[] ValidAtlases;
    public Brush CurrentBrush;
    public bool FlipVertical = true;

    void Awake()
    {
        _atlases = MapLoader.CompileTextures(this.ValidAtlases);
        _sprites = MapLoader.CompileSprites(_atlases);
        this.Quad.Atlases = _atlases;
        this.Quad.Sprites = _sprites;
    }

    void FixedUpdate()
    {
        if (MapEditorInput.Confirm)
        {
            //TODO: Handle different layers
            this.Quad.PlatformsRenderer.SetSpriteIndexForTile(this.Cursor.GridPos.X, this.FlipVertical ? this.Grid.Height - this.Cursor.GridPos.Y - 1 : this.Cursor.GridPos.Y, this.CurrentBrush.SpriteIndex);
        }
    }

    public Texture2D GetAtlasForBrush(Brush brush)
    {
        return _atlases[this.ValidAtlases[brush.Layer].name];
    }

    public Sprite GetSpriteForBrush(Brush brush)
    {
        return _sprites[this.ValidAtlases[brush.Layer].name][brush.SpriteIndex];
    }

    /**
     * Private
     */
    private Dictionary<string, Texture2D> _atlases;
    private Dictionary<string, Sprite[]> _sprites;
}
