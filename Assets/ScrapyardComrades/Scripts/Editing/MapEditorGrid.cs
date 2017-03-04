using UnityEngine;

public class MapEditorGrid : VoBehavior
{
    public int GridSpaceSize = 20;
    public int Width { get { return _width; } }
    public int Height { get { return _height; } }
    
    public void InitializeGridForSize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public IntegerVector GridToWorld(IntegerVector gridPos)
    {
        return gridPos * this.GridSpaceSize;
    }

    public IntegerVector MoveLeft(IntegerVector pos)
    {
        pos.X -= 1;
        if (pos.X < 0)
            pos.X = _width - 1;
        return pos;
    }

    public IntegerVector MoveRight(IntegerVector pos)
    {
        pos.X += 1;
        if (pos.X >= _width)
            pos.X = 0;
        return pos;
    }

    public IntegerVector MoveDown(IntegerVector pos)
    {
        pos.Y -= 1;
        if (pos.Y < 0)
            pos.Y = _height - 1;
        return pos;
    }

    public IntegerVector MoveUp(IntegerVector pos)
    {
        pos.Y += 1;
        if (pos.Y >= _height)
            pos.Y = 0;
        return pos;
    }

    /**
     * Private
     */
    private int _width;
    private int _height;
}
