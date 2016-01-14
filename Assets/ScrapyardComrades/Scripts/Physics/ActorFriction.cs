using UnityEngine;

public class ActorFriction : VoBehavior
{
    public float Friction;
    public string VelocityKey = "";

    void Start()
    {
        _actor = this.GetComponent<Actor2D>();
        _standard = this.VelocityKey == "";
    }

    void Update()
    {
        if (_standard)
        {
            Vector2 v = _actor.Velocity;
            float mag = v.magnitude;
            float newMag = mag - this.Friction * Time.deltaTime;
            v = v.normalized * (newMag > 0.0f ? newMag : 0.0f);
            _actor.Velocity = v;
        }
        else
        {
            VelocityModifier vMod = _actor.GetVelocityModifier(this.VelocityKey);
            if (vMod != null)
            {
                Vector2 v = vMod.Modifier;
                float mag = v.magnitude;
                float newMag = mag - this.Friction * Time.deltaTime;
                v = v.normalized * (newMag > 0.0f ? newMag : 0.0f);
                vMod.Modifier = v;
                _actor.SetVelocityModifier(this.VelocityKey, vMod);
            }
        }
    }

    /**
     * Private
     */
    private Actor2D _actor;
    private bool _standard;
}
