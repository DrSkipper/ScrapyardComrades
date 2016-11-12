using UnityEngine;

public class SCSpriteAnimator : VoBehavior, IPausable
{
    public SCSpriteAnimation DefaultAnimation;
    public SCSpriteAnimation CurrentAnimation { get { guaranteeCurrentAnimation(); return _currentAnimation; } }
    public int Elapsed { get { return _elapsed; } }

    void Awake()
    {
        this.PlayAnimation(this.DefaultAnimation);
        this.localNotifier.Listen(FreezeFrameEvent.NAME, this, freezeFrame);
        this.localNotifier.Listen(FreezeFrameEndedEvent.NAME, this, freezeFrameEnded);
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
        this.spriteRenderer.sprite = _currentAnimation.Frames[0];
    }

    public void GoToFrame(int frame)
    {
        guaranteeCurrentAnimation();
        _frame = Mathf.Clamp(frame, 0, _currentAnimation.Frames.Length - 1);
        _elapsed = Mathf.RoundToInt(_frame * this.GetFrameDuration());
        this.spriteRenderer.sprite = _currentAnimation.Frames[_frame];
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

                this.spriteRenderer.sprite = _currentAnimation.Frames[_frame];
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
        //this.localNotifier.SendEvent(new SCSpriteAnimationLoopEvent(_elapsed));
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

    private void guaranteeCurrentAnimation()
    {
        if (_currentAnimation == null)
            this.PlayAnimation(this.DefaultAnimation);
    }

    private void freezeFrame(LocalEventNotifier.Event e)
    {
        _frozen = true;
    }

    private void freezeFrameEnded(LocalEventNotifier.Event e)
    {
        _frozen = false;
    }
}
