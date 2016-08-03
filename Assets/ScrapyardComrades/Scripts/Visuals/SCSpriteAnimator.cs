using UnityEngine;

public class SCSpriteAnimator : VoBehavior
{
    public SCSpriteAnimation DefaultAnimation;
    public SCSpriteAnimation CurrentAnimation { get { return _currentAnimation; } }

    void Awake()
    {
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
        _elapsed = 0.0f;
        _playing = true;
        this.spriteRenderer.sprite = _currentAnimation.Frames[0];
    }

    public void GoToFrame(int frame)
    {
        if (_currentAnimation == null)
            this.PlayAnimation(this.DefaultAnimation);
        _frame = Mathf.Clamp(frame, 0, _currentAnimation.Frames.Length - 1);
        _elapsed = _frame * this.GetFrameDuration();
        this.spriteRenderer.sprite = _currentAnimation.Frames[_frame];
    }

    void OnValidate()
    {
        if (_currentAnimation == null)
            this.PlayAnimation(this.DefaultAnimation);
    }

    void FixedUpdate()
    {
        if (_playing)
        {
            _elapsed += 1;
            float frameDuration = this.GetFrameDuration();
            float nextFrameTime = _frame >= _currentAnimation.Frames.Length - 1 ? _currentAnimation.LengthInFrames : (_frame + 1) * frameDuration;

            if (_elapsed >= nextFrameTime)
            {
                if (_frame >= _currentAnimation.Frames.Length - 1)
                {
                    if (_looping)
                    {
                        _frame = _currentAnimation.LoopFrame;
                        _elapsed = _currentAnimation.LoopFrame * frameDuration;
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

    /**
     * Private
     */
    private SCSpriteAnimation _currentAnimation;
    private bool _playing;
    private bool _looping;
    private int _frame;
    private float _elapsed;
}
