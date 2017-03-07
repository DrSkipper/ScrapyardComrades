using UnityEngine;
using UnityEngine.UI;

public class WorldEditorQuadVisual : MonoBehaviour
{
    public Text Text;
    public Image Outline;
    public IntegerRect QuadBounds;
    public Color StandardBorderColor;
    public Color SelectedBorderColor;
    
    public void MoveToGridPos(MapEditorGrid grid)
    {
        IntegerVector worldPos = grid.GridToWorld(QuadBounds.Min);
        this.transform.SetPosition2D(worldPos.X, worldPos.Y);
    }

    void OnReturnToPool()
    {
        this.Outline.color = this.StandardBorderColor;
    }

    public void Select()
    {
        this.Outline.color = this.SelectedBorderColor;
    }

    public void UnSelect()
    {
        this.Outline.color = this.StandardBorderColor;
    }
}
