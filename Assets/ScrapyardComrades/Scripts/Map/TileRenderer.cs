using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileRenderer : VoBehavior
{
    public enum TilingMethod
    {
        CPU,
        GPU
    }
    
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public Texture2D Atlas;
    public Sprite[] Sprites;
    public TilingMethod Method = TilingMethod.CPU;
    public bool FlipVertical = true;

    public void CreateEmptyMap(int width, int height)
    {
        this.CreateMapWithGrid(new MapGridSpaceInfo[width, width]);
    }

    public void CreateMapWithGrid(MapGridSpaceInfo[,] grid)
    {
        this.Clear();
        _width = grid.GetLength(0);
        _height = grid.GetLength(1);
        createMapUsingMesh(grid);
    }

    public void Clear()
    {
        this.GetComponent<MeshFilter>().mesh = null;
        this.renderer.material.mainTexture = null;
    }

    public void SetSpriteIndicesForTiles(int[] x, int[] y, int[] spriteIndices)
    {
        setTileSpriteIndicesInMesh(x, y, spriteIndices);
    }

    public void SetSpriteIndexForTile(int tileX, int tileY, int spriteIndex)
    {
        setTileSpriteIndexInMesh(tileX, tileY, spriteIndex);
    }


    /**
     * Private
     */
    private int _width;
    private int _height;

    private void createMapUsingMesh(MapGridSpaceInfo[,] grid)
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
                int spriteIndex = grid[x, y].TileId;

                if (spriteIndex >= this.Sprites.Length || spriteIndex < 0)
                {
                    Debug.LogWarning("Invalid sprite index: " + spriteIndex);
                }

                /*if (!this.Sprites.ContainsKey(this.SpritesByTileId[spriteIndex]))
                {
                    Debug.LogWarning("Couldn't find sprite key: " + this.SpritesByTileId[spriteIndex]);
                }*/

                Vector2[] spriteUVs = this.Sprites[spriteIndex].uv;
                Vector2 bottomLeftUV = spriteUVs[0];
                Vector2 bottomRightUV = spriteUVs[1];
                Vector2 topLeftUV = spriteUVs[2];
                Vector2 topRightUV = spriteUVs[3];

                // Add vertices and vertex data to mesh data
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
                vertices.Add(topLeft);
                vertices.Add(topRight);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
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
        this.GetComponent<MeshFilter>().mesh = mesh;
        this.renderer.material.mainTexture = this.Atlas;
    }

    private void setTileSpriteIndexInMesh(int tileX, int tileY, int spriteIndex)
    {
        int tileIndex = tileY * _width + tileX;
        int startingUVIndex = tileIndex * 4;
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();

        Vector2[] spriteUVs = this.Sprites[spriteIndex].uv;
        Vector2[] uvs = meshFilter.mesh.uv;
        uvs[startingUVIndex] = spriteUVs[0]; // bottom left
        uvs[startingUVIndex + 1] = spriteUVs[1]; // bottom right
        uvs[startingUVIndex + 2] = spriteUVs[2]; // top left
        uvs[startingUVIndex + 3] = spriteUVs[3]; // top right

        meshFilter.mesh.uv = uvs;
    }


    private void setTileSpriteIndicesInMesh(int[] x, int[] y, int[] spriteIndices)
    {
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        Vector2[] uvs = meshFilter.mesh.uv;

        for (int i = 0; i < x.Length; ++i)
        {
            int tileIndex = y[i] * _width + x[i];
            int startingUVIndex = tileIndex * 4;

            Vector2[] spriteUVs = this.Sprites[spriteIndices[i]].uv;
            uvs[startingUVIndex] = spriteUVs[0]; // bottom left
            uvs[startingUVIndex + 1] = spriteUVs[1]; // bottom right
            uvs[startingUVIndex + 2] = spriteUVs[2]; // top left
            uvs[startingUVIndex + 3] = spriteUVs[3]; // top right
        }
        meshFilter.mesh.uv = uvs;
    }
}
