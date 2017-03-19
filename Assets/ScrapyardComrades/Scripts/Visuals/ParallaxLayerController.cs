using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    public ParallaxQuadGroup CurrentLayerVisual;
    public ParallaxQuadGroup PreviousLayerVisual;
    public Transform Tracker;
    public CameraController CameraController;
    public float ParallaxRatio;

    void Update()
    {
        // Move to position ParallaxRatio between origin and tracker (camera) position
        Vector2 origin = this.transform.parent.position;
        Vector2 trackerPos = (Vector2)this.Tracker.transform.position - origin;
        Vector2 trackerNormalized = trackerPos.normalized;
        float magnitude = trackerPos.magnitude * this.ParallaxRatio;
        IntegerVector final = trackerNormalized * magnitude + origin;
        this.transform.position = new Vector3(final.X, final.Y, this.transform.position.z);
    }

    public void TransitionToNewLayer(NewMapInfo.ParallaxLayer layer, int quadWidth)
    {
        this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh, this.CurrentLayerVisual.MeshRenderer.sharedMaterial.mainTexture as Texture2D);

        this.PreviousLayerVisual.transform.SetZ(this.CurrentLayerVisual.transform.position.z);
        this.PreviousLayerVisual.transform.SetLocalY(this.CurrentLayerVisual.transform.localPosition.y);

        if (layer != null)
        {
            this.ParallaxRatio = layer.parallax_ratio;
            Sprite sprite = Resources.Load<Sprite>(layer.sprite_name);
            this.CurrentLayerVisual.CreateMeshForLayer(sprite, layer.loops, layer.height, layer.x_position,  layer.parallax_ratio, quadWidth);
            this.CurrentLayerVisual.transform.SetZ(layer.depth);
        }
        else
        {
            this.CurrentLayerVisual.CreateMeshForLayer(null, false, 0, 0, 0, 0);
        }
    }
}
