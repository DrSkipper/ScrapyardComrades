using UnityEngine;

[ExecuteInEditMode]
public class TilesetEditorManager : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public TilesetData TilesetToEdit;
    public TilesetData CurrentEditingTileset { get; set; }

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

        this.MeshRenderer.sharedMaterial.mainTexture = _texture;
    }

    void Update()
    {

    }

    /**
     * Private
     */
    private Texture2D _texture;
    private Sprite[] _sprites;
}
