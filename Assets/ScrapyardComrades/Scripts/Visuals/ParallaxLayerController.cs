using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    public ParallaxQuadGroup CurrentLayerVisual;
    public ParallaxQuadGroup PreviousLayerVisual;
    public Transform Tracker;
    public float ParallaxRatio;

    void Update()
    {
        // Move to position ParallaxRatio between 0,0 and tracker (camera) position
        Vector2 trackerPos = this.Tracker.transform.position;
        Vector2 trackerNormalized = trackerPos.normalized;
        float magnitude = trackerPos.magnitude * this.ParallaxRatio;
        IntegerVector final = trackerNormalized * magnitude;
        this.transform.position = new Vector3(final.X, final.Y, this.transform.position.z);
    }

    public void TransitionToNewLayer(SCParallaxLayer layer)
    {
        //if (layer.Sprite != this.CurrentLayerVisual.MostRecentSprite)
        //{
            this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh);
            this.CurrentLayerVisual.CreateMeshForLayer(layer);
        //}
    }
}
