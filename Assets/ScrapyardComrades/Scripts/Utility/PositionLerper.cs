using UnityEngine;

public class PositionLerper : MonoBehaviour
{
    public Transform Destination;
    public Easing.Flow EasingFlow;
    public Easing.Function EasingFunction;
    public int Duration;
    public int Delay;
    public bool OnlyX = false;
    public bool OnlyY = false;
    public bool LerpSize = false;
    public bool Loops;
    public bool RunOnStart;
    public bool Running { get { return _running; } }

    void Start()
    {
        if (this.RunOnStart)
            this.BeginLerp();
    }

    public void BeginLerp()
    {
        _ourRectTransform = this.transform as RectTransform;
        if (_ourRectTransform != null)
        {
            _destRectTransform = this.Destination as RectTransform;
            _startingPos = _ourRectTransform.anchoredPosition;
            _startingSize = _ourRectTransform.sizeDelta;
            _sizeDiff = _destRectTransform.sizeDelta - _startingSize;
            _dist = _destRectTransform.anchoredPosition - _ourRectTransform.anchoredPosition;
        }
        else
        {
            _startingPos = this.transform.position;
            _dist = this.Destination.position - this.transform.position;
        }
        _easingDelegate = Easing.GetFunction(this.EasingFunction, this.EasingFlow);
        _t = 0;
        _delayComplete = false;
        _running = true;
    }

    void FixedUpdate()
    {
        if (_running)
        {
            ++_t;

            if (!_delayComplete)
            {
                if (_t >= this.Delay)
                {
                    _delayComplete = true;
                    _t = 0;
                }
            }
            else
            {
                if (_t < this.Duration)
                {
                    float x = !this.OnlyY ? _easingDelegate(_t, _startingPos.x, _dist.x, this.Duration) : 0;
                    float y = !this.OnlyX ? _easingDelegate(_t, _startingPos.y, _dist.y, this.Duration) : 0;
                    setPos(new Vector2(x, y));

                    if (this.LerpSize)
                    {
                        float w = _easingDelegate(_t, _startingSize.x, _sizeDiff.x, this.Duration);
                        float h = _easingDelegate(_t, _startingSize.y, _sizeDiff.y, this.Duration);
                        setSize(new Vector2(w, h));
                    }
                }
                else
                {
                    if (!this.Loops)
                    {
                        _running = false;
                        setPos(_ourRectTransform != null ? _destRectTransform.anchoredPosition : (Vector2)this.Destination.position);

                        if (this.LerpSize)
                            setSize(_destRectTransform.sizeDelta);
                    }
                    else
                    {
                        _t = 0;
                        setPos(_startingPos);

                        if (this.LerpSize)
                            setSize(_startingSize);
                    }
                }
            }
        }
    }

    /**
     * Private
     */
    private RectTransform _ourRectTransform;
    private RectTransform _destRectTransform;
    private Easing.EasingDelegate _easingDelegate;
    private bool _running;
    private int _t;
    private Vector2 _startingPos;
    private Vector2 _startingSize;
    private Vector2 _dist;
    private Vector2 _sizeDiff;
    private bool _delayComplete;

    private void setPos(Vector2 pos)
    {
        if (_ourRectTransform != null)
        {
            if (this.OnlyX)
                _ourRectTransform.anchoredPosition = new Vector2(pos.x, _ourRectTransform.anchoredPosition.y);
            else if (this.OnlyY)
                _ourRectTransform.anchoredPosition = new Vector2(_ourRectTransform.anchoredPosition.x, pos.y);
            else
                _ourRectTransform.anchoredPosition = pos;
        }
        else
        {
            if (this.OnlyX)
                this.transform.SetX(pos.x);
            else if (this.OnlyY)
                this.transform.SetY(pos.y);
            else
                this.transform.SetPosition2D(pos);
        }
    }

    private void setSize(Vector2 size)
    {
        _ourRectTransform.sizeDelta = size;
    }
}
