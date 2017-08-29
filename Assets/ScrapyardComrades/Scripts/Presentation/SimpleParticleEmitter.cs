using UnityEngine;
using System.Collections.Generic;

public class SimpleParticleEmitter : MonoBehaviour, IPausable
{
    public IntegerCollider EmmissionRange;
    public SimpleParticle[] PooledParticles;
    public SCSpriteAnimation[] ParticleAnimations;
    public IntegerVector SpawnIntervalRange;
    public int MaxParticlesToSpawn = -1;

    void Awake()
    {
        _used = new List<SimpleParticle>();
        _unused = new List<SimpleParticle>();
    }

    void OnSpawn()
    {
        _t = Random.Range(this.SpawnIntervalRange.X, this.SpawnIntervalRange.Y + 1);
        _count = 0;
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
        if (this.MaxParticlesToSpawn < 0 || _count < this.MaxParticlesToSpawn)
        {
            if (_t <= 0)
            {
                _unused.Shuffle();
                SimpleParticle particle = _unused.Pop();

                if (particle != null)
                {
                    ++_count;
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
    }

    /**
     * Private
     */
    private List<SimpleParticle> _used;
    private List<SimpleParticle> _unused;
    private int _t;
    private int _count;

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
