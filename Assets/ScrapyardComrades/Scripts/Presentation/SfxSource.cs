using UnityEngine;

public class SfxSource : MonoBehaviour
{
    private const int MAX_VOLUME_DIST = 128;
    private const int MIN_VOLUME_DIST = 850;
    private const int VOLUME_DIST_DIFF = MIN_VOLUME_DIST - MAX_VOLUME_DIST;
    private const float MIN_VOL = 0.01f;

    public AudioSource AudioSource;
    public bool isPlaying { get { return this.AudioSource.isPlaying; } }

    public void Play(AudioClip clip, float volume, float pitch, bool proximity, Transform proximityTarget)
    {
        _usingProximity = proximity;
        _proximityTarget = proximity ? proximityTarget : null;
        _maxVolume = volume;

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
    private Transform _proximityTarget;
    private float _maxVolume;

    private float getProximityVolume()
    {
        if (_proximityTarget == null || PlayerReference.Transform == null)
            return 0.0f;

        int d = Mathf.RoundToInt(PlayerReference.Transform.Distance2D(_proximityTarget));

        if (d <= MAX_VOLUME_DIST)
            return _maxVolume;
        if (d >= MIN_VOLUME_DIST)
            return 0.0f;

        return Mathf.Lerp(_maxVolume, MIN_VOL, (d - MAX_VOLUME_DIST) / (float)VOLUME_DIST_DIFF);
    }
}
