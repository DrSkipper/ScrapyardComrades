using UnityEngine;
using UnityEngine.UI;

public class MapEditorBorder : MonoBehaviour, CameraBoundsHandler
{
    public RectTransform RectTransform;
    public MapEditorGrid Grid;
    public IntegerRectCollider CurrentQuadBoundsCheck;
    public CameraController CameraController;

    public IntegerRectCollider GetBounds() { return this.CurrentQuadBoundsCheck; }

    void FixedUpdate()
    {
        Vector2 newSize = new Vector2(this.Grid.Width * this.Grid.GridSpaceSize, this.Grid.Height * this.Grid.GridSpaceSize);
        if (newSize != this.RectTransform.sizeDelta)
        {
            this.RectTransform.sizeDelta = newSize;
            this.CurrentQuadBoundsCheck.Size = this.RectTransform.sizeDelta;
            this.CurrentQuadBoundsCheck.Offset = new IntegerVector(this.RectTransform.sizeDelta / 2.0f);
            this.CameraController.CalculateBounds();
        }
    }
}
