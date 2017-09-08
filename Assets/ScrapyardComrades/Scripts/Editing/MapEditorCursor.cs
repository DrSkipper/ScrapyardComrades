using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class MapEditorCursor : VoBehavior, IPausable
{
    public MapEditorGrid Grid;
    public Image HighlightImage;
    public IntegerVector GridPos;
    public Color StandardColor;
    public Color EraserColor;
    public bool Hidden { get { return _hidden; } }

    void Start()
    {
        MenuInput.EnableMenuInput();
    }

    void FixedUpdate()
    {
        if (!_hidden)
        {
            if (MenuInput.NavLeft)
                this.GridPos = this.Grid.MoveLeft(this.GridPos);
            else if (MenuInput.NavRight)
                this.GridPos = this.Grid.MoveRight(this.GridPos);
            else if (MenuInput.NavDown)
                this.GridPos = this.Grid.MoveDown(this.GridPos);
            else if (MenuInput.NavUp)
                this.GridPos = this.Grid.MoveUp(this.GridPos);

            this.MoveToGridPos();
        }
    }

    public void MoveToGridPos()
    {
        IntegerVector worldPos = this.Grid.GridToWorld(this.GridPos);
        this.transform.SetPosition2D(worldPos.X, worldPos.Y);
    }

    public void Hide()
    {
        _hidden = true;
        if (this.HighlightImage != null)
            this.HighlightImage.enabled = false;
    }

    public void UnHide()
    {
        _hidden = false;
        if (this.HighlightImage != null)
            this.HighlightImage.enabled = true;
    }

    public void EnableEraser(bool enable)
    {
        this.HighlightImage.color = enable ? this.EraserColor : this.StandardColor;
    }

    /** 
     * private
     */
    private bool _hidden;
}
