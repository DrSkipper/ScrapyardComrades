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

    public void TransitionToNewLayer(SCParallaxLayer layer, int quadWidth)
    {
        this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh);
        this.CurrentLayerVisual.CreateMeshForLayer(layer, this.ParallaxRatio, quadWidth);
        this.PreviousLayerVisual.transform.SetLocalY(this.CurrentLayerVisual.transform.localPosition.y);
        this.CurrentLayerVisual.transform.SetLocalY(Mathf.RoundToInt(this.CameraController.CameraViewHeight * (layer != null ? layer.Height : 0.5f) - this.CameraController.CameraViewHeight / 2));
    }
}
