using UnityEngine;

public class ScreenEffectTrigger : MonoBehaviour
{
    public MaterialLerper MaterialLerper;
    public string EventName = "MUTATE";
    public SimpleCameraShaderEffect CameraEffect;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(this.EventName, this, onMutate);
        this.MaterialLerper.OnComplete = onLerpComplete;
    }

    private void onMutate(LocalEventNotifier.Event e)
    {
        this.CameraEffect.enabled = true;
        this.MaterialLerper.BeginLerp();
    }

    private void onLerpComplete()
    {
        this.CameraEffect.enabled = false;
    }
}
