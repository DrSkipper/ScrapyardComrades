using UnityEngine;

public class MoveAndFadeParticleEmitter : AbstractParticleEmitter, IPausable
{
    protected override void EmitParticle(GameObject go, IntegerVector pos)
    {
        go.GetComponent<MoveAndFadeParticle>().Emit(this.OnParticleComplete);
    }
}
