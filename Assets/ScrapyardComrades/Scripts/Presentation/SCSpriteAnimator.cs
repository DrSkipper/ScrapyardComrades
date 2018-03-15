using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SCSpriteAnimator : VoBehavior, IPausable
{
    public SCSpriteAnimation DefaultAnimation;
    public SCSpriteAnimation CurrentAnimation { get { guaranteeCurrentAnimation(); return _currentAnimation; } }
    public int Elapsed { get { return _elapsed; } }
    public bool IsPlaying { get { return _playing; } }
    public bool PlayDefaultOnSpawn = true;

    void Awake()
    {
        if (this.DefaultAnimation != null)
            this.PlayAnimation(this.DefaultAnimation);
        this.localNotifier.Listen(FreezeFrameEvent.NAME, this, freezeFrame);
        this.localNotifier.Listen(FreezeFrameEndedEvent.NAME, this, freezeFrameEnded);
    }

    void OnSpawn()
    {
        if (this.PlayDefaultOnSpawn)
            this.PlayAnimation(this.DefaultAnimation);
    }

    public void PlayAnimation(SCSpriteAnimation animation)
    {
        this.PlayAnimation(animation, animation.LoopsByDefault);
    }

    public void PlayAnimation(SCSpriteAnimation animation, bool loop)
    {
        _currentAnimation = animation;
        _looping = loop;
        _frame = 0;
        _elapsed = 0;
        _playing = true;
        updateVisual();
    }

    public void GoToFrame(int frame)
    {
        guaranteeCurrentAnimation();
        _frame = Mathf.Clamp(frame, 0, _currentAnimation.Frames.Length - 1);
        _elapsed = Mathf.RoundToInt(_frame * this.GetFrameDuration());
        updateVisual();
    }

    void OnValidate()
    {
        guaranteeCurrentAnimation();
    }

    void FixedUpdate()
    {
        if (_playing && !_frozen)
        {
            _elapsed += 1;
            float frameDuration = this.GetFrameDuration();
            int nextFrameTime = _frame >= _currentAnimation.Frames.Length - 1 ? _currentAnimation.LengthInFrames : Mathf.RoundToInt((_frame + 1) * frameDuration);

            if (_elapsed >= nextFrameTime)
            {
                if (_frame >= _currentAnimation.Frames.Length - 1)
                {
                    if (_looping)
                    {
                        this.Loop(_currentAnimation.LoopFrame, frameDuration);
                    }
                    else
                    {
                        _playing = false;
                    }
                }
                else
                {
                    ++_frame;
                }

                updateVisual();
            }
        }
    }

    public float GetFrameDuration()
    {
        return ((float)_currentAnimation.LengthInFrames) / ((float)_currentAnimation.Frames.Length);
    }

    public int GetDataFrameForVisualFrame(int visualFrame)
    {
        return Mathf.Clamp(Mathf.RoundToInt(this.GetFrameDuration() * (float)Mathf.Clamp(visualFrame, 0, _currentAnimation.Frames.Length - 1)), 0, _currentAnimation.LengthInFrames - 1);
    }

    public void Loop(int frame = 0, float frameDuration = -1)
    {
        if (frameDuration < 0.0f)
            frameDuration = this.GetFrameDuration();
        _frame = frame;
        _elapsed = Mathf.RoundToInt(frame * frameDuration);
    }

    public void Stop()
    {
        _playing = false;
    }

    public void Play()
    {
        if (_currentAnimation == null)
            this.PlayAnimation(this.DefaultAnimation);
        else
            this.PlayAnimation(_currentAnimation);
    }

    public void Continue()
    {
        _playing = true;
    }

    /**
     * Private
     */
    private SCSpriteAnimation _currentAnimation;
    private bool _playing;
    private bool _looping;
    private int _frame;
    private int _elapsed;
    private bool _frozen = false;

    private void updateVisual()
    {
        this.spriteRenderer.sprite = _currentAnimation.Frames[_frame];
        if (_currentAnimation.Sfx != null && Application.isPlaying && (_frame == _currentAnimation.SfxFrame || (_currentAnimation.SfxFrame2 > _currentAnimation.SfxFrame && _frame == _currentAnimation.SfxFrame2)))
            SoundManager.Play(_currentAnimation.Sfx.name);
    }

    private void guaranteeCurrentAnimation()
    {
        if (_currentAnimation == null && this.DefaultAnimation != null)
            this.PlayAnimation(this.DefaultAnimation);
    }

    public void freezeFrame(LocalEventNotifier.Event e)
    {
        _frozen = true;
    }

    public void freezeFrameEnded(LocalEventNotifier.Event e)
    {
        _frozen = false;
    }
}
