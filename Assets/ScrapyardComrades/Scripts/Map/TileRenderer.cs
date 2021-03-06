﻿using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileRenderer : VoBehavior
{
    public MeshFilter MeshFilter;
    public int TileRenderSize = 20;
    public Texture2D Atlas;
    public bool FlipVertical = false;
    public bool FlipUvsVertical = false;

    void Awake()
    {
        if (this.Atlas != null)
        {
            this.renderer.material.mainTexture = this.Atlas;
            _sprites = Texture2DExtensions.GetSprites(TilesetData.TILESETS_PATH, this.Atlas.name);
        }
    }

    public void SetAtlas(string atlasName)
    {
        this.Atlas = IndexedSpriteManager.GetAtlas(TilesetData.TILESETS_PATH, atlasName);
        _sprites = Texture2DExtensions.GetSprites(TilesetData.TILESETS_PATH, atlasName);
        this.renderer.material.mainTexture = this.Atlas;
    }

    public void CreateEmptyMap(int width, int height)
    {
        this.CreateMapWithGrid(new NewMapInfo.MapTile[width, width]);
    }

    public void CreateMapWithGrid(NewMapInfo.MapTile[,] grid)
    {
        if (_cleared)
        {
            _width = grid.GetLength(0);
            _height = grid.GetLength(1);
            createMapUsingMesh(grid);
        }
        else if (_width != grid.GetLength(0) || _height != grid.GetLength(1))
        {
            this.Clear();
            _width = grid.GetLength(0);
            _height = grid.GetLength(1);
            createMapUsingMesh(grid);
        }
        else
        {
            Vector2[] uvs = this.MeshFilter.mesh.uv;

            for (int y = 0; y < grid.GetLength(0); ++y)
            {
                for (int x = 0; x < grid.GetLength(1); ++x)
                {
                    int tileIndex = y * _width + x;
                    int startingUVIndex = tileIndex * 4;

                    string spriteName = grid[x, y].sprite_name;
                    Vector2[] spriteUVs = _sprites.ContainsKey(spriteName) ? _sprites[spriteName].uv : EMPTY_UVS;
                    uvs[startingUVIndex] = spriteUVs[0]; // bottom left
                    uvs[startingUVIndex + 1] = spriteUVs[1]; // bottom right
                    uvs[startingUVIndex + 2] = spriteUVs[2]; // top left
                    uvs[startingUVIndex + 3] = spriteUVs[3]; // top right
                }
            }

            this.MeshFilter.mesh.uv = uvs;
        }
    }

    public void Clear()
    {
        if (!_cleared)
        {
            this.MeshFilter.mesh = null;
            //this.renderer.material.mainTexture = null;
        }
    }

    public void SetSpriteIndicesForTiles(int[] x, int[] y, string[] spriteNames)
    {
        setTileSpriteIndicesInMesh(x, y, spriteNames);
    }

    public void SetSpriteIndexForTile(int tileX, int tileY, string spriteName)
    {
        setTileSpriteIndexInMesh(tileX, tileY, spriteName);
    }
    
    /**
     * Private
     */
    private int _width;
    private int _height;
    private bool _cleared = true;
    private Dictionary<string, Sprite> _sprites;
    private static Vector2[] EMPTY_UVS = new Vector2[] { new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1) };

    private void createMapUsingMesh(NewMapInfo.MapTile[,] grid)
    {
        float originX = 0; // this.transform.position.x;
        float originY = 0; // this.transform.position.y;
        float originZ = 0; // this.transform.position.z;

        int numTiles = _width * _height;
        int numTriangles = numTiles * 2;

        // Generate mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] triangles = new int[numTriangles * 3]; // Clockwise order of vertices within triangles (for correct render direction)
        
        float finalY = originY + _height * this.TileRenderSize;

        for (int y = 0; y < _height; ++y)
        {
            for (int x = 0; x < _width; ++x)
            {
                int tileIndex = _width * y + x;
                int triangleIndex = tileIndex * 2 * 3;
                string spriteName = grid[x, y].sprite_name;

                // Create 4 verts
                float smallY = this.FlipVertical ? finalY - y * this.TileRenderSize : originY + y * this.TileRenderSize;
                float bigY = this.FlipVertical ? smallY - this.TileRenderSize : smallY + this.TileRenderSize;
                Vector3 bottomLeft = new Vector3(originX + x * this.TileRenderSize, smallY, originZ);
                Vector3 bottomRight = new Vector3(bottomLeft.x + this.TileRenderSize, bottomLeft.y, originZ);
                Vector3 topLeft = new Vector3(bottomLeft.x, bigY, originZ);
                Vector3 topRight = new Vector3(bottomRight.x, topLeft.y, originZ);

                // Indices of verts
                int bottomLeftVert = vertices.Count;
                int bottomRightVert = bottomLeftVert + 1;
                int topLeftVert = bottomRightVert + 1;
                int topRightVert = topLeftVert + 1;

                // Assign vert indices to triangles
                triangles[triangleIndex] = topLeftVert;
                triangles[triangleIndex + 1] = bottomRightVert;
                triangles[triangleIndex + 2] = bottomLeftVert;

                triangles[triangleIndex + 3] = topLeftVert;
                triangles[triangleIndex + 4] = topRightVert;
                triangles[triangleIndex + 5] = bottomRightVert;

                // Handle UVs
                Vector2[] spriteUVs = _sprites.ContainsKey(spriteName) ? _sprites[spriteName].uv : EMPTY_UVS;

                Vector2 bottomLeftUV = spriteUVs[this.FlipUvsVertical ? 2 : 0];
                Vector2 bottomRightUV = spriteUVs[this.FlipUvsVertical ? 3 : 1];
                Vector2 topLeftUV = spriteUVs[this.FlipUvsVertical ? 0 : 2];
                Vector2 topRightUV = spriteUVs[this.FlipUvsVertical ? 1 : 3];

                // Add vertices and vertex data to mesh data
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
                vertices.Add(topLeft);
                vertices.Add(topRight);
                normals.Add(Vector3.back);
                normals.Add(Vector3.back);
                normals.Add(Vector3.back);
                normals.Add(Vector3.back);
                uvs.Add(bottomLeftUV);
                uvs.Add(bottomRightUV);
                uvs.Add(topLeftUV);
                uvs.Add(topRightUV);
            }
        }

        // Populate a mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles;

        // Assign mesh to behaviors
        this.MeshFilter.mesh = mesh;
        //this.renderer.material.mainTexture = this.Atlas;
    }

    private void setTileSpriteIndexInMesh(int tileX, int tileY, string spriteName)
    {
        int tileIndex = tileY * _width + tileX;
        int startingUVIndex = tileIndex * 4;

        Vector2[] spriteUVs = _sprites.ContainsKey(spriteName) ? _sprites[spriteName].uv : EMPTY_UVS;
        Vector2[] uvs = this.MeshFilter.mesh.uv;
        uvs[startingUVIndex] = spriteUVs[this.FlipUvsVertical ? 2 : 0]; // bottom left
        uvs[startingUVIndex + 1] = spriteUVs[this.FlipUvsVertical ? 3 : 1]; // bottom right
        uvs[startingUVIndex + 2] = spriteUVs[this.FlipUvsVertical ? 0 : 2]; // top left
        uvs[startingUVIndex + 3] = spriteUVs[this.FlipUvsVertical ? 1 : 3]; // top right

        this.MeshFilter.mesh.uv = uvs;
    }
    
    private void setTileSpriteIndicesInMesh(int[] x, int[] y, string[] spriteNames)
    {
        Vector2[] uvs = this.MeshFilter.mesh.uv;

        for (int i = 0; i < x.Length; ++i)
        {
            int tileIndex = y[i] * _width + x[i];
            int startingUVIndex = tileIndex * 4;

            Vector2[] spriteUVs = _sprites.ContainsKey(spriteNames[i]) ? _sprites[spriteNames[i]].uv : EMPTY_UVS;
            uvs[startingUVIndex + (this.FlipUvsVertical ? 2 : 0)] = spriteUVs[0]; // bottom left
            uvs[startingUVIndex + (this.FlipUvsVertical ? 3 : 1)] = spriteUVs[1]; // bottom right
            uvs[startingUVIndex + (this.FlipUvsVertical ? 0 : 2)] = spriteUVs[2]; // top left
            uvs[startingUVIndex + (this.FlipUvsVertical ? 1 : 3)] = spriteUVs[3]; // top right
        }
        this.MeshFilter.mesh.uv = uvs;
    }
}
