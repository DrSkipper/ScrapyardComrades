using UnityEngine;
using Rewired;

public class MapEditorCursor : VoBehavior
{
    public MapEditorGrid Grid;
    public IntegerVector GridPos;

    void Start()
    {
        ReInput.players.GetPlayer(0).controllers.maps.SetMapsEnabled(true, "Menu");
    }

    void FixedUpdate()
    {
        if (MapEditorInput.NavLeft)
            this.GridPos = this.Grid.MoveLeft(this.GridPos);
        else if (MapEditorInput.NavRight)
            this.GridPos = this.Grid.MoveRight(this.GridPos);
        else if (MapEditorInput.NavDown)
            this.GridPos = this.Grid.MoveDown(this.GridPos);
        else if (MapEditorInput.NavUp)
            this.GridPos = this.Grid.MoveUp(this.GridPos);

        IntegerVector worldPos = this.Grid.GridToWorld(this.GridPos);
        this.transform.SetPosition2D(worldPos.X, worldPos.Y);
    }
}
