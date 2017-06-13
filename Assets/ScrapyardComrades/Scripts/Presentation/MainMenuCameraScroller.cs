using UnityEngine;

public class MainMenuCameraScroller : VoBehavior, IPausable
{
    public float Speed = 4;
    public Transform BottomLeftOuter;
    public Transform BottomLeftInner;
    public Transform TopRightOuter;
    public Transform TopRightInner;
    public float LeftRightChance = 0.5f;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(MainMenuBGSwapEvent.NAME, this, BeginNewLerp);
    }

    void FixedUpdate()
    {
        this.transform.SetPosition2D(Vector2.MoveTowards(this.transform.position, _target, this.Speed));
    }

    public void BeginNewLerp(LocalEventNotifier.Event e)
    {
        Vector2 start, end;

        if (Random.value <= this.LeftRightChance)
        {
            start = new Vector2(Random.Range(this.BottomLeftOuter.position.x, this.BottomLeftInner.position.x), Random.Range(this.BottomLeftOuter.position.y, this.TopRightOuter.position.y));
            end = new Vector2(Random.Range(this.TopRightInner.position.x, this.TopRightOuter.position.x), Random.Range(this.BottomLeftOuter.position.y, this.TopRightOuter.position.y));
        }
        else
        {
            start = new Vector2(Random.Range(this.BottomLeftOuter.position.x, this.TopRightOuter.position.x), Random.Range(this.BottomLeftOuter.position.y, this.BottomLeftInner.position.y));
            end = new Vector2(Random.Range(this.BottomLeftOuter.position.x, this.TopRightOuter.position.x), Random.Range(this.TopRightInner.position.y, this.TopRightOuter.position.y));
        }

        if (Random.value > 0.5f)
        {
            Vector2 temp = start;
            start = end;
            end = temp;
        }

        this.transform.SetPosition2D(start);
        _target = end;
    }

    /**
     * Private
     */
    private Vector2 _target;
}
