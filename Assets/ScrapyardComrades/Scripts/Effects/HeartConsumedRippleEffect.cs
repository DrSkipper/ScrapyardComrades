using UnityEngine;

public class HeartConsumedRippleEffect : MonoBehaviour
{
    public Material RippleMaterial;
    public SimpleCameraShaderEffect Effect;
    public Camera Camera;
    public CameraController CameraController;
    public string BoundsVariableName = "_Bounds";
    public string TimeVariableName = "_T";
    public string IntensityVariableName = "_Intensity";
    public Vector2 Size;
    public float Duration = 2.0f;
    public float TimeMultiplier = 1.0f;
    public float MaxIntensity = 0.05f;
    public float MinIntensity = 0.0f;
    public float TimeToMaxIntensity = 0.1f;
    public float FadeOutTime = 0.3f;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(HeartConsumedEvent.NAME, this, onHeartConsume);
    }

    void Update()
    {
        if (_running)
        {
            _t += Time.deltaTime;
            updateVisual();

            if (_t > this.Duration)
            {
                _t = 0.0f;
                updateVisual(true);
                _running = false;
                this.Effect.enabled = false;
            }
        }
    }

    /**
     * Private
     */
    private float _t;
    private bool _running = false;
    private Vector2 _effectPos;

    private void onHeartConsume(LocalEventNotifier.Event e)
    {
        _effectPos = (e as HeartConsumedEvent).Position;
        this.Effect.enabled = true;
        _running = true;
        _t = 0.0f;
        updateVisual();
    }

    private void updateVisual(bool wipePosition = false)
    {
        Vector2 pos;
        
        if (wipePosition)
        {
            pos = Vector2.zero;
        }
        else if (this.Camera.targetTexture != null)
        {
            pos = _effectPos - ((Vector2)this.CameraController.transform.position - new Vector2(this.CameraController.CameraViewWidth / 2.0f, this.CameraController.CameraViewHeight / 2.0f));
            pos.x /= (float)this.CameraController.CameraViewWidth;
            pos.y /= (float)this.CameraController.CameraViewHeight;
        }
        else
        {
            pos = this.Camera.WorldToScreenPoint(_effectPos);
            pos.x /= Screen.width;
            pos.y /= Screen.height;
        }

        float intensity = this.MaxIntensity;
        if (_t >= this.TimeToMaxIntensity)
        {
            if (_t > this.Duration - this.FadeOutTime)
            {
                intensity = Mathf.Lerp(this.MinIntensity, this.MaxIntensity, (this.Duration - _t) / this.FadeOutTime);
            }
        }
        else
        {
            intensity = Mathf.Lerp(this.MinIntensity, this.MaxIntensity, _t / this.TimeToMaxIntensity);
        }

        this.RippleMaterial.SetFloat(this.IntensityVariableName, intensity);
        this.RippleMaterial.SetVector(this.BoundsVariableName, new Vector4(pos.x, pos.y, this.Size.x, this.Size.y));
        this.RippleMaterial.SetFloat(this.TimeVariableName, _t * this.TimeMultiplier);
    }
}
