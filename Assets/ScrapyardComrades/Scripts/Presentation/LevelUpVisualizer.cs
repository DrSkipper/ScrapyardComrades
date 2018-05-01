using UnityEngine;

public class LevelUpVisualizer : MonoBehaviour, IPausable
{
    public SpriteRenderer Image;
    public Sprite PrevSprite;
    public Sprite NextSprite;
    public int Duration = 150;
    public int InitialInterval = 20;
    public int FinalInterval = 3;
    public int NextInterval = 3;
    public int FinalDelay = 20;
    public Easing.Flow EasingFlow;
    public Easing.Function EasingFunction;
    //public int SizeMult = 8;
    public bool Running { get { return _running; } }

    public void Run()
    {
        //_prev = this.Levels[Mathf.Clamp(SaveData.PlayerStats.Level, 0, this.Levels.Length - 1)];
        //_next = this.Levels[Mathf.Clamp(SaveData.PlayerStats.Level + 1, 0, this.Levels.Length - 1)];
        setSprite(this.PrevSprite);
        //this.Image.enabled = true;
        _counter = 0;
        _interval = 0;
        _nextInterval = this.InitialInterval;
        _running = true;
        _easing = Easing.GetFunction(this.EasingFunction, this.EasingFlow);
    }

    void FixedUpdate()
    {
        if (_running)
        {
            ++_counter;
            ++_interval;

            if (_counter >= this.Duration)
            {
                if (this.Image.sprite != this.NextSprite)
                {
                    setSprite(this.NextSprite);
                    _interval = 0;
                    _nextInterval = this.FinalDelay;
                }

                if (_interval > this.FinalDelay)
                {
                    _running = false;
                    //this.Image.enabled = false;
                }
            }

            else if (_interval >= _nextInterval)
            {
                _interval = 0;
                if (this.Image.sprite == this.PrevSprite)
                {
                    setSprite(this.NextSprite);
                    _nextInterval = this.NextInterval;
                }
                else
                {
                    setSprite(this.PrevSprite);
                    _nextInterval = Mathf.RoundToInt(_easing(_counter, this.InitialInterval, this.FinalInterval - this.InitialInterval, this.Duration));
                }
            }
        }
    }

    void OnReturnToPool()
    {
        _running = false;
    }

    /**
     * Private
     */
    //private Sprite _prev;
    //private Sprite _next;
    private bool _running;
    private int _counter;
    private int _interval;
    private int _nextInterval;
    private Easing.EasingDelegate _easing;

    private void setSprite(Sprite s)
    {
        this.Image.sprite = s;
        /*RectTransform imageTransform = this.Image.transform as RectTransform;
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.rect.width * this.SizeMult);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.rect.height * this.SizeMult);*/
    }
}
