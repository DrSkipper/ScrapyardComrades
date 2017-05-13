using UnityEngine;

public class ParallaxLayerController : VoBehavior, IPausable
{
    public const string PARALLAX_PATH = "Parallax/";
    public ParallaxQuadGroup CurrentLayerVisual;
    public ParallaxQuadGroup PreviousLayerVisual;
    public ParallaxLayerController PreviousLayerController;
    public Transform Tracker;
    public float ParallaxRatio;
    public Shader LitShader;
    public Shader UnlitShader;

    void Update()
    {
        // Move to position ParallaxRatio between origin and tracker (camera) position
        IntegerVector origin = (Vector2)this.transform.parent.position;
        IntegerVector trackerPos = ((IntegerVector)(Vector2)this.Tracker.transform.position) - origin;
        Vector2 trackerNormalized = ((Vector2)trackerPos).normalized;
        float magnitude = ((Vector2)trackerPos).magnitude * this.ParallaxRatio;
        IntegerVector final = ((IntegerVector)(trackerNormalized * magnitude)) + origin;
        this.transform.position = new Vector3(final.X, final.Y, this.transform.position.z);
    }

    public void TransitionToNewLayer(NewMapInfo.ParallaxLayer layer, int quadWidth)
    {
        this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh, this.CurrentLayerVisual.MeshRenderer.material.mainTexture as Texture2D, this.CurrentLayerVisual.MeshRenderer.material.shader);
        this.PreviousLayerController.ParallaxRatio = this.ParallaxRatio;
        this.PreviousLayerVisual.transform.SetLocalPosition2D(this.CurrentLayerVisual.transform.localPosition.x, this.CurrentLayerVisual.transform.localPosition.y);
        this.PreviousLayerVisual.transform.SetZ(this.CurrentLayerVisual.transform.position.z - 1);

        this.PreviousLayerVisual.MeshRenderer.sortingLayerName = this.CurrentLayerVisual.transform.position.z > MapEditorManager.PLATFORMS_LAYER_DEPTH ? MapEditorManager.PARALLAX_BACK_SORT_LAYER : MapEditorManager.PARALLAX_FRONT_SORT_LAYER;

        this.PreviousLayerVisual.gameObject.layer = this.CurrentLayerVisual.gameObject.layer;

        if (layer != null)
        {
            this.CurrentLayerVisual.gameObject.layer = LayerMask.NameToLayer(layer.GetLayerName());

            this.ParallaxRatio = layer.parallax_ratio;
            Sprite sprite = Resources.Load<Sprite>(PARALLAX_PATH + layer.sprite_name);
            this.CurrentLayerVisual.CreateMeshForLayer(sprite, layer.loops, layer.height, layer.x_position,  layer.parallax_ratio, quadWidth, layer.lit ? this.LitShader : this.UnlitShader);
            this.CurrentLayerVisual.transform.SetZ(layer.depth);

            this.CurrentLayerVisual.MeshRenderer.sortingLayerName = layer.depth > MapEditorManager.PLATFORMS_LAYER_DEPTH ? MapEditorManager.PARALLAX_BACK_SORT_LAYER : MapEditorManager.PARALLAX_FRONT_SORT_LAYER;
        }
        else
        {
            this.CurrentLayerVisual.CreateMeshForLayer(null, false, 0, 0, 0, 0, null);
        }
    }
}
