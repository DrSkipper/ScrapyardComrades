using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class MapEditorCursor : VoBehavior
{
    public MapEditorGrid Grid;
    public Image ContentsImage;
    public Image HighlightImage;
    public Color StandardColor;
    public Color EraserColor;
    public Sprite EraserSprite;
    public IntegerVector GridPos;

    void Start()
    {
        ReInput.players.GetPlayer(0).controllers.maps.SetMapsEnabled(true, "Menu");
    }

    void FixedUpdate()
    {
        if (!_hidden)
        {
            if (MapEditorInput.NavLeft)
                this.GridPos = this.Grid.MoveLeft(this.GridPos);
            else if (MapEditorInput.NavRight)
                this.GridPos = this.Grid.MoveRight(this.GridPos);
            else if (MapEditorInput.NavDown)
                this.GridPos = this.Grid.MoveDown(this.GridPos);
            else if (MapEditorInput.NavUp)
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
        if (this.ContentsImage != null)
            this.ContentsImage.enabled = false;
        if (this.HighlightImage != null)
            this.HighlightImage.enabled = false;
    }

    public void UnHide()
    {
        _hidden = false;
        if (this.ContentsImage != null)
            this.ContentsImage.enabled = true;
        if (this.HighlightImage != null)
            this.HighlightImage.enabled = true;
    }

    public void ChangeBrushContents(Sprite brushSprite, bool eraser)
    {
        if (!eraser)
        {
            this.ContentsImage.sprite = brushSprite;
            this.HighlightImage.color = this.StandardColor;
        }
        else
        {
            this.ContentsImage.sprite = this.EraserSprite;
            this.HighlightImage.color = this.EraserColor;
        }
    }

    /** 
     * private
     */
    private bool _hidden;
}
