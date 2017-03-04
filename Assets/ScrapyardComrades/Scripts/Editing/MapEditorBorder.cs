using UnityEngine;
using UnityEngine.UI;

public class MapEditorBorder : MonoBehaviour
{
    public RectTransform RectTransform;
    public MapEditorGrid Grid;

    void FixedUpdate()
    {
        this.RectTransform.sizeDelta = new Vector2(this.Grid.Width * this.Grid.GridSpaceSize, this.Grid.Height * this.Grid.GridSpaceSize);
    }
}
