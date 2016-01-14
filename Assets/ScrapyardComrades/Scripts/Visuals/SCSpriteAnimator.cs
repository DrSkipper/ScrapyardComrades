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

    void Update()
    {
        if (_playing)
        {
            _elapsed += SCPhysics.DeltaFrames;
            float nextFrameTime = _frame >= _currentAnimation.Frames.Length - 1 ? _currentAnimation.LengthInFrames : (_frame + 1) * (((float)_currentAnimation.LengthInFrames) / ((float)_currentAnimation.Frames.Length));

            if (_elapsed >= nextFrameTime)
            {
                if (_frame >= _currentAnimation.Frames.Length - 1)
                {
                    if (_looping)
                    {
                        _frame = 0;
                        _elapsed = 0.0f;
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

    /**
     * Private
     */
    private SCSpriteAnimation _currentAnimation;
    private bool _playing;
    private bool _looping;
    private int _frame;
    private float _elapsed;
}
