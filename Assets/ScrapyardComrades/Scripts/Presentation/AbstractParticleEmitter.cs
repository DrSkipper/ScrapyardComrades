using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractParticleEmitter : MonoBehaviour
{
    public GameObject[] PooledParticles;
    public IntegerCollider EmmissionRange;
    public IntegerVector SpawnIntervalRange;
    public int MaxParticlesToSpawn = -1;
    public bool StartOnAwake = false;
    public bool Paused = false;
    public delegate void OnCompleteDelegate(GameObject particle);

    void Awake()
    {
        _used = new List<GameObject>();
        _unused = new List<GameObject>();

        if (this.StartOnAwake)
            this.OnSpawn();
    }

    void OnSpawn()
    {
        _t = Random.Range(this.SpawnIntervalRange.X, this.SpawnIntervalRange.Y + 1);
        _count = 0;
        _used.Clear();
        _unused.Clear();

        for (int i = 0; i < this.PooledParticles.Length; ++i)
        {
            this.PooledParticles[i].SetActive(false);
            _unused.Add(this.PooledParticles[i]);
        }
    }
    
    void FixedUpdate()
    {
        if (!this.Paused && (this.MaxParticlesToSpawn < 0 || _count < this.MaxParticlesToSpawn))
        {
            if (_t <= 0)
            {
                _unused.Shuffle();
                GameObject particle = _unused.Pop();

                if (particle != null)
                {
                    ++_count;
                    _t = Random.Range(this.SpawnIntervalRange.X, this.SpawnIntervalRange.Y + 1);

                    IntegerVector pos = new IntegerVector(Random.Range(this.EmmissionRange.Bounds.Min.X, this.EmmissionRange.Bounds.Max.X), Random.Range(this.EmmissionRange.Bounds.Min.Y, this.EmmissionRange.Bounds.Max.Y));

                    particle.SetActive(true);
                    particle.transform.SetPosition2D(pos);
                    this.EmitParticle(particle, pos);

                    _unused.Remove(particle);
                    if (!_used.Contains(particle))
                        _used.Add(particle);
                }
            }
            else
            {
                _t -= 1;
            }
        }
    }

    public void OnParticleComplete(GameObject particle)
    {
        _used.Remove(particle);
        if (!_unused.Contains(particle))
            _unused.Add(particle);

        particle.SetActive(false);
    }
    
    protected abstract void EmitParticle(GameObject go, IntegerVector pos);
    protected List<GameObject> _used;
    protected List<GameObject> _unused;
    protected int _t;
    protected int _count;
}

public interface AbstractParticle
{
    void Emit(SCSpriteAnimation animation, AbstractParticleEmitter.OnCompleteDelegate onComplete);
}
