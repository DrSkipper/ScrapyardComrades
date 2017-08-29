using UnityEngine;
using System.Collections.Generic;

public class SimpleParticleEmitter : MonoBehaviour, IPausable
{
    public IntegerCollider EmmissionRange;
    public SimpleParticle[] PooledParticles;
    public SCSpriteAnimation[] ParticleAnimations;
    public IntegerVector SpawnIntervalRange;

    void Awake()
    {
        _used = new List<SimpleParticle>();
        _unused = new List<SimpleParticle>();
    }

    void OnSpawn()
    {
        _t = Random.Range(this.SpawnIntervalRange.X, this.SpawnIntervalRange.Y + 1);
        _used.Clear();
        _unused.Clear();

        for (int i = 0; i < this.PooledParticles.Length; ++i)
        {
            this.PooledParticles[i].gameObject.SetActive(false);
            _unused.Add(this.PooledParticles[i]);
        }
    }

    void FixedUpdate()
    {
        if (_t <= 0)
        {
            _unused.Shuffle();
            SimpleParticle particle = _unused.Pop();

            if (particle != null)
            {
                _t = Random.Range(this.SpawnIntervalRange.X, this.SpawnIntervalRange.Y + 1);

                IntegerVector pos = new IntegerVector(Random.Range(this.EmmissionRange.Bounds.Min.X, this.EmmissionRange.Bounds.Max.X), Random.Range(this.EmmissionRange.Bounds.Min.Y, this.EmmissionRange.Bounds.Max.Y));
                emitParticle(particle, pos);
            }
        }
        else
        {
            _t -= 1;
        }
    }

    /**
     * Private
     */
    private List<SimpleParticle> _used;
    private List<SimpleParticle> _unused;
    private int _t;

    private void emitParticle(SimpleParticle particle, IntegerVector pos)
    {
        particle.gameObject.SetActive(true);
        particle.transform.SetPosition2D(pos);
        particle.Emit(this.ParticleAnimations[Random.Range(0, this.ParticleAnimations.Length)], onParticleComplete);
        _unused.Remove(particle);
        if (!_used.Contains(particle))
            _used.Add(particle);
    }

    private void onParticleComplete(SimpleParticle particle)
    {
        _used.Remove(particle);
        if (!_unused.Contains(particle))
            _unused.Add(particle);

        particle.gameObject.SetActive(false);
    }
}
