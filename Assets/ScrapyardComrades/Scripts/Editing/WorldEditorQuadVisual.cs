using UnityEngine;
using UnityEngine.UI;

public class WorldEditorQuadVisual : MonoBehaviour
{
    public Text Text;
    public Image Outline;
    public Image Contents;
    public IntegerRect QuadBounds;
    public Color StandardBorderColor;
    public Color SelectedBorderColor;
    public Color FilledColor;
    public Color EmptyColor;
    public string PreviewLayerName;
    public string QuadName { get; private set; }
    
    public void ConfigureForQuad(string name, MapInfo quadInfo, int worldGridSpaceSize, int gridSpaceRenderSize, IntegerVector quadPos)
    {
        this.QuadName = name;
        this.Text.text = name;
        IntegerVector size = new IntegerVector(quadInfo.width / worldGridSpaceSize, quadInfo.height / worldGridSpaceSize);
        IntegerRect bounds = new IntegerRect(IntegerVector.Zero, size);
        bounds.Min = quadPos;
        bounds.Max = quadPos + size;
        this.QuadBounds = bounds;
        ((RectTransform)this.transform).sizeDelta = new Vector2((size.X) * gridSpaceRenderSize, (size.Y) * gridSpaceRenderSize);

        if (_texture == null)
            _texture = new Texture2D(quadInfo.width, quadInfo.height, TextureFormat.ARGB32, false);
        else
            _texture.Resize(quadInfo.width, quadInfo.height, TextureFormat.ARGB32, false);

        MapInfo.MapLayer previewLayer = quadInfo.GetLayerWithName(this.PreviewLayerName);
        MapGridSpaceInfo[,] data = previewLayer.GetGrid(quadInfo.tilesets);

        for (int x = 0; x < quadInfo.width; ++x)
        {
            for (int y = 0; y < quadInfo.height; ++y)
            {
                _texture.SetPixel(x, y, data[x, quadInfo.height - 1 - y].TileId != 0 ? this.FilledColor : this.EmptyColor);
            }
        }

        _texture.filterMode = FilterMode.Point;
        _texture.Apply();

        Sprite sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
        this.Contents.sprite = sprite;
    }

    public void MoveToGridPos(MapEditorGrid grid)
    {
        IntegerVector worldPos = grid.GridToWorld(QuadBounds.Min);
        this.transform.SetPosition2D(worldPos.X, worldPos.Y);
    }

    void OnReturnToPool()
    {
        this.Outline.color = this.StandardBorderColor;
    }

    public void Select()
    {
        this.Outline.color = this.SelectedBorderColor;
    }

    public void UnSelect()
    {
        this.Outline.color = this.StandardBorderColor;
    }

    /**
     * Private
     */
    Texture2D _texture;
}
