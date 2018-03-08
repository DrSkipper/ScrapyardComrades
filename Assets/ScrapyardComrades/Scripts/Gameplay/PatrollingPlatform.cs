using UnityEngine;
using System.Collections.Generic;

public class PatrollingPlatform : VoBehavior, IMovingPlatform, IPausable
{
    public int Speed = 2;
    public Transform Start;
    public Transform Destination;
    public LayerMask BlockingMask;
    public LayerMask ActorMask;
    public TileRenderer Renderer;
    
    [HideInInspector]
    public int Width = 1;
    [HideInInspector]
    public int Height = 1;
    [HideInInspector]
    public TilesetData Tileset;

    public Vector2 Velocity
    {
        get
        {
            if (_blocked || _stopped)
                return Vector2.zero;
            else if (_outward)
                return _outwardVelocity;
            else
                return _inwardVelocity;
        }
    }

    void Awake()
    {
        _collisions = new List<GameObject>();
        int size = PatrollingPlatformConfigurer.MAX_SIZE + 2;
        _fakeTiles = new NewMapInfo.MapTile[size, size];

        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                _fakeTiles[x, y] = new NewMapInfo.MapTile();
            }
        }
    }

    void OnSpawn()
    {
        createPlatform();
        this.integerCollider.AddToCollisionPool();
        this.transform.SetPosition2D(this.Start.position);
        _actualPos = this.transform.position;
        _outward = true;
        _outwardVelocity = (((Vector2)this.Destination.transform.position) - ((Vector2)this.Start.transform.position)).normalized * this.Speed;
        _inwardVelocity = (((Vector2)this.Start.transform.position) - ((Vector2)this.Destination.transform.position)).normalized * this.Speed;

        // Hack check to see if we're not in the level editor
        if (EntityTracker.Instance != null)
        {
            // Disable debug image for our destination
            SpriteRenderer destinationRenderer = this.Destination.GetComponent<SpriteRenderer>();
            if (destinationRenderer != null)
                destinationRenderer.enabled = false;
        }
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        if (!_stopped)
        {
            IntegerVector target = _outward ? (Vector2)this.Destination.transform.position : (Vector2)this.Start.transform.position;
            Vector2 nextActualPos = Vector2.MoveTowards(_actualPos, target, this.Speed);
            IntegerVector nextPos = nextActualPos;
            IntegerVector currentPos = this.integerPosition;
            int offsetX = nextPos.X - currentPos.X;
            int offsetY = nextPos.Y - currentPos.Y;

            // Can we move or are we blocked?
            bool canMove = true;
            this.integerCollider.Collide(_collisions, offsetX, offsetY, this.BlockingMask);

            if (_collisions.Count > 0)
            {
                DirectionalVector2 dir = _outward ? new DirectionalVector2(_outwardVelocity.x, _outwardVelocity.y) : new DirectionalVector2(_inwardVelocity.x, _inwardVelocity.y);
                for (int i = 0; i < _collisions.Count; ++i)
                {
                    // Push actors to make room if possible
                    Actor2D collision = _collisions[i].GetComponent<Actor2D>();
                    if (collision == null || !collision.Push(dir, this.integerCollider, offsetX, offsetY))
                    {
                        canMove = false;
                        break;
                    }
                }
                _collisions.Clear();
            }

            // If we can move, do so, otherwise register that we're blocked here
            if (canMove)
            {
                // If we're moving a negative y value, get a list of the actors on us to pull down with us
                if (offsetY < 0 || offsetX != 0)
                {
                    this.integerCollider.Collide(_collisions, offsetX == 0 ? 0 : -Mathf.RoundToInt(Mathf.Sign(offsetX)), offsetY == 0 ? 0 : 1, this.ActorMask);
                }

                _blocked = false;
                this.transform.SetPosition2D(nextPos);
                _actualPos = nextActualPos;

                if (nextPos == target)
                {
                    //TODO: Stop for a bit
                    _outward = !_outward;
                    _actualPos = nextPos;
                }

                // Pull any actors on our platform down with us
                if ((offsetY < 0 || offsetX != 0) && _collisions.Count > 0)
                {
                    for (int i = 0; i < _collisions.Count; ++i)
                    {
                        _collisions[i].GetComponent<Actor2D>().Move(new Vector2(offsetX, offsetY));
                    }
                    _collisions.Clear();
                }
            }
            else
            {
                _blocked = true;
                _actualPos = currentPos;
            }
        }
    }

    /**
     * Private
     */
    private Vector2 _actualPos;
    private Vector2 _outwardVelocity;
    private Vector2 _inwardVelocity;
    private bool _blocked;
    private bool _stopped;
    private bool _outward;
    private List<GameObject> _collisions;
    NewMapInfo.MapTile[,] _fakeTiles;

    private void createPlatform()
    {
        if (this.Tileset != null)
        {
            this.Renderer.SetAtlas(this.Tileset.AtlasName);
            
            Dictionary<TilesetData.TileType, List<TilesetData.SpriteData>> autotileDict = this.Tileset.GetAutotileDictionary();
            Dictionary<string, TilesetData.SpriteData> spriteDict = this.Tileset.GetSpriteDataDictionary();
            string filledSpriteName = autotileDict[TilesetData.TileType.Surrounded][0].SpriteName;
            string emptySpritename = this.Tileset.GetEmptySpriteName();

            int lengthX = _fakeTiles.GetLength(0);
            int diffX = lengthX - this.Width;
            int halfDiffXLarge = Mathf.RoundToInt(diffX / 2.0f + 0.1f);
            int halfDiffXSmall = diffX / 2;

            int lengthY = _fakeTiles.GetLength(1);
            int diffY = lengthY - this.Height;
            int halfDiffYLarge = Mathf.RoundToInt(diffY / 2.0f + 0.1f);
            int halfDiffYSmall = diffY / 2;

            for (int x = 0; x < lengthX; ++x)
            {
                for (int y = 0; y < lengthY; ++y)
                {
                    bool withinX = x >= halfDiffXSmall && x < lengthX - halfDiffXLarge;
                    bool withinY = y >= halfDiffYSmall && y < lengthY - halfDiffYLarge;

                    if (withinX && withinY)
                    {
                        _fakeTiles[x, y].is_filled = true;
                        _fakeTiles[x, y].sprite_name = filledSpriteName;
                    }
                    else
                    {
                        _fakeTiles[x, y].is_filled = false;
                        _fakeTiles[x, y].sprite_name = emptySpritename;
                    }
                }
            }

            for (int x = 0; x < lengthX; ++x)
            {
                for (int y = 0; y < lengthY; ++y)
                {
                    _fakeTiles[x, y].sprite_name = TilesetData.GetAutotileSpriteName(TilesetData.GetAutotileType(x, y, _fakeTiles, spriteDict, false), autotileDict, this.Tileset);
                }
            }

            this.Renderer.CreateMapWithGrid(_fakeTiles);

            int offsetX = this.Width % 2 != 0 ? this.Renderer.TileRenderSize / 2 : 0;
            offsetX -= lengthX * this.Renderer.TileRenderSize / 2;
            int offsetY = this.Height % 2 != 0 ? this.Renderer.TileRenderSize / 2 : 0;
            offsetY -= lengthY * this.Renderer.TileRenderSize / 2;
            this.Renderer.transform.SetLocalPosition2D(offsetX, offsetY);
        }

        (this.integerCollider as IntegerRectCollider).Size = new IntegerVector(this.Width * this.Renderer.TileRenderSize, this.Height * this.Renderer.TileRenderSize);
    }
}
