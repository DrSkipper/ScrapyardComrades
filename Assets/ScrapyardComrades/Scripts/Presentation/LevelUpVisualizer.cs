using UnityEngine;
using UnityEngine.UI;

public class LevelUpVisualizer : MonoBehaviour, IPausable
{
    public Image Image;
    public Sprite[] Levels;
    public int Duration = 150;
    public int InitialInterval = 20;
    public int FinalInterval = 3;
    public int FinalDelay = 20;
    public int SizeMult = 8;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerHealthController.MUTATE_EVENT, this, onMutate);
    }

    void FixedUpdate()
    {
        if (_running)
        {
            ++_counter;
            ++_interval;

            if (_counter >= this.Duration)
            {
                if (this.Image.sprite != _next)
                {
                    setSprite(_next);
                    _interval = 0;
                    _nextInterval = this.FinalDelay;
                }

                if (_interval > this.FinalDelay)
                {
                    _running = false;
                    this.Image.enabled = false;
                }
            }

            else if (_interval >= _nextInterval)
            {
                _interval = 0;
                _nextInterval = Mathf.RoundToInt(Mathf.Lerp(this.InitialInterval, this.FinalInterval, (float)_counter / this.Duration));
                setSprite(this.Image.sprite == _prev ? _next : _prev);
            }
        }
    }

    /**
     * Private
     */
    private Sprite _prev;
    private Sprite _next;
    private bool _running;
    private int _counter;
    private int _interval;
    private int _nextInterval;

    private void onMutate(LocalEventNotifier.Event e)
    {
        _prev = this.Levels[Mathf.Clamp(SaveData.PlayerStats.Level, 0, this.Levels.Length - 1)];
        _next = this.Levels[Mathf.Clamp(SaveData.PlayerStats.Level + 1, 0, this.Levels.Length - 1)];
        setSprite(_prev);
        this.Image.enabled = true;
        _counter = 0;
        _interval = 0;
        _nextInterval = this.InitialInterval;
        _running = true;
    }

    private void setSprite(Sprite s)
    {
        this.Image.sprite = s;
        RectTransform imageTransform = this.Image.transform as RectTransform;
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.rect.width * this.SizeMult);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.rect.height * this.SizeMult);
    }
}
