﻿using UnityEngine;
using System.Collections.Generic;

public class ParallaxQuadGroup : VoBehavior
{
    public CameraController CameraController;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;
    public Sprite MostRecentSprite { get { return _mostRecentSprite; } }

    public void UpdateWithMesh(Mesh mesh, Texture2D texture, Shader shader)
    {
        if (shader != null && shader != this.MeshRenderer.material.shader)
            this.MeshRenderer.material.shader = shader;

        this.MeshFilter.mesh = mesh;
        this.MeshRenderer.material.mainTexture = texture;
    }

    public void CreateMeshForLayer(Sprite sprite, bool loops, float height, float xPos, float parallaxRatio, int quadWidth, Shader shader)
    {
        if (sprite == null)
        {
            initializeLists();
            this.MeshFilter.mesh = null;
            return;
        }

        if (shader != null && shader != this.MeshRenderer.material.shader)
            this.MeshRenderer.material.shader = shader;

        this.transform.SetLocalPosition2D(Mathf.RoundToInt(this.CameraController.CameraViewWidth * xPos - this.CameraController.CameraViewWidth / 2), Mathf.RoundToInt(this.CameraController.CameraViewHeight * height - this.CameraController.CameraViewHeight / 2));

        int numQuads = 1;
        float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
        float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
        float originX = 0.0f;
        float originY = 0.0f;
        float originZ = 0.0f;
        _mostRecentSprite = sprite;
        Vector2[] spriteUVs = sprite.GetUVs();
        Vector3 normal = Vector3.back;

        // If layer is looped, get number of quads we need
        if (loops)
        {
            int cameraWidth = this.CameraController.CameraViewWidth;
            // Account for extra loops needed due to parallax ratio
            int extraWidth = Mathf.RoundToInt((quadWidth - cameraWidth) * (1.0f - parallaxRatio));
            float floatQuadCount = (cameraWidth + extraWidth) / spriteWidth;
            numQuads = Mathf.RoundToInt(floatQuadCount);

            // We want an odd number of quads (so layer is centered), and want to make sure we round up so there isn't blank space on sides.
            if (numQuads % 2 == 0)
            {
                ++numQuads;
            }
            else if (floatQuadCount > numQuads)
            {
                ++numQuads;
                if (numQuads % 2 == 0)
                    ++numQuads;
            }
        }

        // Create mesh
        initializeLists();
        float minY = originY - spriteHeight / 2.0f;
        float maxY = originY + spriteHeight / 2.0f;
        float minX = originX - (spriteWidth * numQuads) / 2.0f;
        float currentX = minX;
        Vector2 bottomLeftUV = spriteUVs[0];
        Vector2 bottomRightUV = spriteUVs[1];
        Vector2 topLeftUV = spriteUVs[2];
        Vector2 topRightUV = spriteUVs[3];

        for (int quad = 0; quad < numQuads; ++quad)
        {
            // Create 4 verts
            Vector3 bottomLeft = new Vector3(currentX, minY, originZ);
            Vector3 bottomRight = new Vector3(currentX + spriteWidth, minY, originZ);
            Vector3 topLeft = new Vector3(currentX, maxY, originZ);
            Vector3 topRight = new Vector3(currentX + spriteWidth, maxY, originZ);

            // Indices of verts
            int bottomLeftVert = _verts.Count;
            int bottomRightVert = bottomLeftVert + 1;
            int topLeftVert = bottomLeftVert + 2;
            int topRightVert = bottomLeftVert + 3;
            _verts.Add(bottomLeft);
            _verts.Add(bottomRight);
            _verts.Add(topLeft);
            _verts.Add(topRight);

            // Triangles
            _tris.Add(topLeftVert);
            _tris.Add(bottomRightVert);
            _tris.Add(bottomLeftVert);

            _tris.Add(topLeftVert);
            _tris.Add(topRightVert);
            _tris.Add(bottomRightVert);

            // UVs and normals
            _uvs.Add(bottomLeftUV);
            _uvs.Add(bottomRightUV);
            _uvs.Add(topLeftUV);
            _uvs.Add(topRightUV);
            _norms.Add(normal);
            _norms.Add(normal);
            _norms.Add(normal);
            _norms.Add(normal);

            // Update currentX
            currentX += spriteWidth;
        }

        // Populate mesh
        Mesh mesh = new Mesh();
        mesh.vertices = _verts.ToArray();
        mesh.normals = _norms.ToArray();
        mesh.uv = _uvs.ToArray();
        mesh.triangles = _tris.ToArray();
        this.MeshRenderer.material.mainTexture = sprite.texture;
        this.MeshFilter.mesh = mesh;
    }

    /**
     * Private
     */
    private List<Vector3> _verts;
    private List<Vector3> _norms;
    private List<Vector2> _uvs;
    private List<int> _tris;
    private Sprite _mostRecentSprite;

    private void initializeLists()
    {
        if (_verts == null)
            _verts = new List<Vector3>();
        else
            _verts.Clear();

        if (_norms == null)
            _norms = new List<Vector3>();
        else
            _norms.Clear();

        if (_uvs == null)
            _uvs = new List<Vector2>();
        else
            _uvs.Clear();

        if (_tris == null)
            _tris = new List<int>();
        else
            _tris.Clear();
    }
}
