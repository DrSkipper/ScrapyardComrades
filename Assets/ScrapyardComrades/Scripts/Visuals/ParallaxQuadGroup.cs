using UnityEngine;
using System.Collections.Generic;

public class ParallaxQuadGroup : MonoBehaviour
{
    public CameraController CameraController;
    public MeshFilter MeshFilter;
    public Sprite MostRecentSprite { get { return _mostRecentSprite; } }

    public void UpdateWithMesh(Mesh mesh)
    {
        this.MeshFilter.mesh = mesh;
    }

    public void CreateMeshForLayer(SCParallaxLayer layer)
    {
        if (layer == null)
        {
            initializeLists();
            this.MeshFilter.mesh = null;
            return;
        }

        int numQuads = 1;
        float spriteWidth = layer.Sprite.rect.width / layer.Sprite.pixelsPerUnit;
        float spriteHeight = layer.Sprite.rect.height / layer.Sprite.pixelsPerUnit;
        float originX = 0.0f;
        float originY = 0.0f;
        float originZ = 0.0f;
        _mostRecentSprite = layer.Sprite;
        Vector2[] spriteUVs = layer.Sprite.GetUVs();
        Vector3 normal = Vector3.back;

        //TODO: Add more loops after determining max distance layer can move from center of vision
        // If layer is looped, get number of quads we need
        if (layer.LoopsHorizontally)
        {
            int cameraWidth = this.CameraController.CameraViewWidth;
            float floatQuadCount = cameraWidth / spriteWidth;
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
