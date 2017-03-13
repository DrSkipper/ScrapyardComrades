using UnityEngine;
using System.Collections.Generic;

public class MapEditorLayer
{
    public string Name;
    public LayerType Type;
    public int Depth;

    public enum LayerType
    {
        Tiles,
        Objects,
        Parallax
    }
}

public class MapEditorTilesLayer : MapEditorLayer
{
    public string TilesetName;
    public NewMapInfo.MapTile[,] Data;

    public MapEditorTilesLayer(NewMapInfo.MapLayer mapLayer, int depth)
    {
        this.Name = mapLayer.name;
        this.Type = LayerType.Tiles;
        this.Depth = depth;
        this.TilesetName = mapLayer.tileset_name;
        this.Data = mapLayer.GetDataGrid();
    }
}

public class MapEditorObjectsLayer : MapEditorLayer
{
    public List<GameObject> Objects;

    public MapEditorObjectsLayer(string name, int depth, List<GameObject> objects)
    {
        this.Name = name;
        this.Type = LayerType.Objects;
        this.Depth = depth;
        this.Objects = objects;
    }
}

public class MapEditorParallaxLayer : MapEditorLayer
{
    public string SpriteName;
    public bool Loops;
    public float Height;

    public MapEditorParallaxLayer(NewMapInfo.ParallaxLayer parallaxLayer, string name)
    {
        this.Name = name;
        this.Type = LayerType.Parallax;
        this.Depth = parallaxLayer.depth;
        this.SpriteName = parallaxLayer.sprite_name;
        this.Loops = parallaxLayer.loops;
        this.Height = parallaxLayer.height;
    }
}
