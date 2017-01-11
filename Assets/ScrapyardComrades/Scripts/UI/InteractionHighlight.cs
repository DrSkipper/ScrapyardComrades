using UnityEngine;

public class InteractionHighlight : MonoBehaviour
{
    public Animator Animator;
    public string TargetChangedTrigger;
    public string HasTargetParam;
    public int YOffset;

    [HideInInspector]
    public float HoverOffset;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(InteractionTargetChangeEvent.NAME, this, interactionTargetChanged);
    }

    void Update()
    {
        if (_target != null)
            this.transform.SetPosition2D(_target.position.x, _target.position.y + this.YOffset + this.HoverOffset);
        else if (_previousTarget != null)
            this.transform.SetPosition2D(_previousTarget.position.x, _previousTarget.position.y + this.YOffset + this.HoverOffset);
        else
            this.transform.SetPosition2D(100000, 100000);
    }

    /**
     * Private
     */
    private Transform _target;
    private Transform _previousTarget;

    private void interactionTargetChanged(LocalEventNotifier.Event e)
    {
        GameObject go = (e as InteractionTargetChangeEvent).Target;
        _previousTarget = _target;
        _target = go == null ? null : go.transform;
        this.HoverOffset = 0;
        this.Animator.SetTrigger(this.TargetChangedTrigger);
        this.Animator.SetBool(this.HasTargetParam, _target != null);
    }
}
