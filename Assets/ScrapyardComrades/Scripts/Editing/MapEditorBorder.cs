using UnityEngine;

public class MapEditorBorder : MonoBehaviour, CameraBoundsHandler
{
    public RectTransform RectTransform;
    public MapEditorGrid Grid;
    public IntegerRectCollider CurrentQuadBoundsCheck;
    public CameraController CameraController;
    public bool UseFullSize = true;
    public string CurrentQuadName { get { return StringExtensions.EMPTY; } }
    public string PrevQuadName { get { return StringExtensions.EMPTY; } }

    public IntegerRectCollider GetBounds() { return this.CurrentQuadBoundsCheck; }

    public void SetMinMax(IntegerVector min, IntegerVector max)
    {
        this.UseFullSize = false;
        _min = min;
        _max = max;
        _newPos = true;
    }

    void FixedUpdate()
    {
        int w = this.UseFullSize ? this.Grid.Width : _max.X - _min.X + 1;
        int h = this.UseFullSize ? this.Grid.Height : _max.Y - _min.Y + 1;
        Vector2 newSize = new Vector2(w * this.Grid.GridSpaceSize, h * this.Grid.GridSpaceSize);
        if (_newPos || newSize != this.RectTransform.sizeDelta)
        {
            _newPos = false;
            if (!this.UseFullSize)
            {
                this.RectTransform.anchoredPosition = new Vector2(_min.X * this.Grid.GridSpaceSize, _min.Y * this.Grid.GridSpaceSize);
            }
            this.RectTransform.sizeDelta = newSize;
            if (this.CurrentQuadBoundsCheck != null)
            {
                this.CurrentQuadBoundsCheck.Size = this.RectTransform.sizeDelta;
                this.CurrentQuadBoundsCheck.Offset = new IntegerVector(this.RectTransform.sizeDelta / 2.0f);
            }
            if (this.CameraController != null)
            {
                this.CameraController.CalculateBounds();
            }
        }
    }

    /**
     * Private
     */
    private IntegerVector _min;
    private IntegerVector _max;
    private bool _newPos;
}
