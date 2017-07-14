using UnityEngine;

public class CharacterSpeedVisualizer : VoBehavior
{
    public SpriteRenderer[] SpeedFrameVisualizers;
    public float MinSpeedForVisualization = 5.0f;
    public float YFactorMultiplier = 0.5f;
    public int FramesBetweenRenders = 4;
    public float AlphaSpeed = 0.1f;

    void Awake()
    {
        _totalFramesToRemember = this.FramesBetweenRenders * this.SpeedFrameVisualizers.Length;
        _spriteHistory = new Sprite[_totalFramesToRemember];
        _positionHistory = new IntegerVector[_totalFramesToRemember];
        _alphaDecrememnt = 1.0f / (this.SpeedFrameVisualizers.Length + 1);
        _targetAlphas = new float[this.SpeedFrameVisualizers.Length];
        removeVisual();
    }

    private void FixedUpdate()
    {
        Vector2 v = this.Actor.TotalVelocity;
        v.y *= this.YFactorMultiplier;
        /*else if (_currentRememberedFrames > 0)
        {
            removeVisual();
        }*/

        _currentRememberedFrames = Mathf.Min(_currentRememberedFrames + 1, _totalFramesToRemember);
        bubbleFrames();
        _spriteHistory[0] = this.spriteRenderer.sprite;
        _positionHistory[0] = this.integerPosition;
        updateVisual(v.magnitude > this.MinSpeedForVisualization);
        updateAlphas();
    }

    void OnReturnToPool()
    {
        if (_currentRememberedFrames > 0)
            removeVisual();
    }

    /**
     * Private
     */
    private int _currentRememberedFrames;
    private int _totalFramesToRemember;
    private Sprite[] _spriteHistory;
    private IntegerVector[] _positionHistory;
    private float _alphaDecrememnt;
    private float[] _targetAlphas;

    private void bubbleFrames()
    {
        for (int i = _currentRememberedFrames - 1; i > 0; --i)
        {
            _spriteHistory[i] = _spriteHistory[i - 1];
            _positionHistory[i] = _positionHistory[i - 1];
        }
    }

    private void updateVisual(bool useAlphas)
    {
        int numUsed = _currentRememberedFrames / this.FramesBetweenRenders;
        float ad = _alphaDecrememnt * this.spriteRenderer.color.a;
        float a = 0.0f + numUsed * ad;
        for (int i = 0; i < this.SpeedFrameVisualizers.Length; ++i)
        {
            int frame = (i + 1) * this.FramesBetweenRenders - 1;
            this.SpeedFrameVisualizers[i].sprite = _spriteHistory[frame];
            this.SpeedFrameVisualizers[i].transform.SetPosition2D(_positionHistory[frame]);

            if (useAlphas && i < numUsed)
            {
                //this.SpeedFrameVisualizers[i].enabled = true;
                _targetAlphas[i] = a;
                a -= ad;

            }
            else //if (this.SpeedFrameVisualizers[i].enabled)
            {
                _targetAlphas[i] = 0.0f;
                //this.SpeedFrameVisualizers[i].enabled = false;
            }
        }
    }

    private void removeVisual()
    {
        _currentRememberedFrames = 0;
        for (int i = 0; i < this.SpeedFrameVisualizers.Length; ++i)
        {
            _targetAlphas[i] = 0.0f;
            //this.SpeedFrameVisualizers[i].enabled = false;
        }
    }

    private void updateAlphas()
    {
        for (int i = 0; i < this.SpeedFrameVisualizers.Length; ++i)
        {
            Color c = this.SpeedFrameVisualizers[i].color;
            c.a = c.a.Approach(_targetAlphas[i], this.AlphaSpeed);
            this.SpeedFrameVisualizers[i].color = c;
        }
    }
}
