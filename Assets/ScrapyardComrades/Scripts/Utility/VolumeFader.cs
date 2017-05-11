using UnityEngine;

public class VolumeFader : MonoBehaviour
{
    public AudioSource AudioSource;

    public void BeginFade(int duration, float targetVolume)
    {
        _duration = duration;
        _targetVolume = targetVolume;
        _t = 0;
        _stepAmount = Mathf.Abs(_targetVolume - this.AudioSource.volume) / _duration;
        _isFading = true;
    }

    void FixedUpdate()
    {
        if (_isFading)
        {
            if (_t < _duration)
            {
                this.AudioSource.volume = this.AudioSource.volume.Approach(_targetVolume, _stepAmount);
                ++_t;
            }
            else
            {
                _isFading = false;
                this.AudioSource.volume = _targetVolume;
            }
        }
    }

    /**
     * Private
     */
    private bool _isFading;
    private int _duration;
    private int _t;
    private float _targetVolume;
    private float _stepAmount;
}
