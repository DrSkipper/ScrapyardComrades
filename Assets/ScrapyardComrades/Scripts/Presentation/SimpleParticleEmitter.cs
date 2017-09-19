using UnityEngine;

public class SimpleParticleEmitter : AbstractParticleEmitter, IPausable
{
    public SCSpriteAnimation[] ParticleAnimations;
    
    protected override void EmitParticle(GameObject go, IntegerVector pos)
    {
        go.GetComponent<SimpleParticle>().Emit(this.ParticleAnimations[Random.Range(0, this.ParticleAnimations.Length)], this.OnParticleComplete);
    }
}
