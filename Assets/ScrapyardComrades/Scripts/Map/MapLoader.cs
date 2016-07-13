﻿using UnityEngine;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public MapGeometryCreator GeometryCreator;
    public string PlatformsLayer = "platforms";

    void Start()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.MapName);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        int[,] grid = mapInfo.GetLayerWithName(this.PlatformsLayer).Grid;
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        this.GeometryCreator.CreateGeometryForGrid(grid);
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
}
