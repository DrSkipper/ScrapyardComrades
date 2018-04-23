using UnityEngine;

public class LevelUpVisualizer : VoBehavior, IPausable
{
    public Sprite[] Levels;
    public int Duration = 150;
    public int InitialInterval = 20;
    public int FinalInterval = 3;
    public int FinalDelay = 20;

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
                if (this.spriteRenderer.sprite != _next)
                {
                    this.spriteRenderer.sprite = _next;
                    _interval = 0;
                    _nextInterval = this.FinalDelay;
                }

                if (_interval > this.FinalDelay)
                {
                    _running = false;
                    this.spriteRenderer.enabled = false;
                }
            }

            else if (_interval >= _nextInterval)
            {
                _interval = 0;
                _nextInterval = Mathf.RoundToInt(Mathf.Lerp(this.InitialInterval, this.FinalInterval, (float)_counter / this.Duration));
                this.spriteRenderer.sprite = this.spriteRenderer.sprite == _prev ? _next : _prev;
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
        this.spriteRenderer.sprite = _prev;
        this.spriteRenderer.enabled = true;
        _counter = 0;
        _interval = 0;
        _nextInterval = this.InitialInterval;
        _running = true;
    }
}
