using UnityEngine;

public class SfxSource : MonoBehaviour
{
    private const int MAX_VOLUME_DIST = 128;
    private const int MIN_VOLUME_DIST = 512;
    private const int VOLUME_DIST_DIFF = MAX_VOLUME_DIST - MIN_VOLUME_DIST;

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

        float d = PlayerReference.Transform.Distance2D(_proximityTarget);

        if (d <= MIN_VOLUME_DIST)
            return 1.0f;
        if (d >= MAX_VOLUME_DIST)
            return 0.0f;

        return Mathf.Lerp(1.0f, 0.0f, (d - MIN_VOLUME_DIST) / VOLUME_DIST_DIFF);
    }
}
