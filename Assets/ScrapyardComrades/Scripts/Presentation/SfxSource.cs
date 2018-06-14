using UnityEngine;

public class SfxSource : MonoBehaviour
{
    public const int MAX_VOLUME_DIST = 64;
    public const int MIN_VOLUME_DIST = 750;
    private const float MIN_VOL = 0.01f;

    public AudioSource AudioSource;
    public bool isPlaying { get { return this.AudioSource.isPlaying; } }

    public void Play(AudioClip clip, float volume, float pitch, bool proximity, Transform proximityTarget, int proxClose, int proxFar)
    {
        _usingProximity = proximity;
        _maxVolume = volume;

        if (proximity)
        {
            _proximityTarget = proximityTarget;
            _proxClose = proxClose;
            _proxFar = proxFar;
            volume = getProximityVolume();
        }
        else
        {
            _proximityTarget = null;
        }

        this.AudioSource.Stop();
        this.AudioSource.clip = clip;
        this.AudioSource.volume = volume;
        this.AudioSource.pitch = pitch;
        this.AudioSource.Play();
    }

    void FixedUpdate()
    {
        if (_usingProximity && this.AudioSource.isPlaying)
        {
            this.AudioSource.volume = getProximityVolume();
        }
    }

    /**
     * Private
     */
    private bool _usingProximity;
    private int _proxClose;
    private int _proxFar;
    private Transform _proximityTarget;
    private float _maxVolume;

    private float getProximityVolume()
    {
        if (_proximityTarget == null || PlayerReference.Transform == null)
            return 0.0f;

        int d = Mathf.RoundToInt(PlayerReference.Transform.Distance2D(_proximityTarget));

        if (d <= _proxClose)
            return _maxVolume;
        if (d >= _proxFar)
            return 0.0f;

        return Easing.QuadEaseInOut(d - _proxClose, _maxVolume, MIN_VOL - _maxVolume, _proxFar - _proxClose);
    }
}
