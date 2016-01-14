using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PerlinShake))]
public class ShakeHandler : VoBehavior
{
    [System.Serializable]
    public struct ShakeTier
    {
        public float TierCutoff;
        public float ShakeAmount;
        public float ShakeDuration;
        public float ShakeSpeed;
    }

    public ShakeTier[] Tiers;
    public int TestTier = -1;

    void Start()
    {
        _shaker = this.GetComponent<PerlinShake>();
    }

    public void ApplyImpact(float impactMagnitude)
    {
        for (int i = this.Tiers.Length - 1; i >= 0; --i)
        {
            if (impactMagnitude >= this.Tiers[i].TierCutoff)
            {
                _currentShakes.Add(new ShakeEntry(impactMagnitude, this.Tiers[i].ShakeDuration));
                break;
            }
        }

        this.InitiateShake();
    }

    void Update()
    {
        for (int i = 0; i < _currentShakes.Count; )
        {
            ShakeEntry shake = _currentShakes[i];
            shake.remainingDuration -= Time.deltaTime;
            if (shake.remainingDuration <= 0.0f)
                _currentShakes.RemoveAt(i);
            else
                ++i;
        }
    }

    public void InitiateShake()
    {
        ShakeTier? tier = null;
        if (this.TestTier >= 0)
        {
            tier = this.Tiers[this.TestTier];
        }
        else
        {
            float totalMagnitude = 0.0f;
            foreach (ShakeEntry shake in _currentShakes)
            {
                totalMagnitude += shake.impactMagnitude;
            }

            for (int i = this.Tiers.Length - 1; i >= 0; --i)
            {
                tier = this.Tiers[i];
                if (totalMagnitude >= tier.Value.TierCutoff)
                    break;
            }
        }

        if (tier.HasValue)
            _shaker.PlayShake(tier.Value.ShakeDuration, tier.Value.ShakeSpeed, tier.Value.ShakeAmount);
    }
    
    /**
     * Private
     */
    private class ShakeEntry
    {
        public float impactMagnitude;
        public float remainingDuration;

        public ShakeEntry(float impactMag, float duration)
        {
            this.impactMagnitude = impactMag;
            this.remainingDuration = duration;
        }
    }

    private List<ShakeEntry> _currentShakes = new List<ShakeEntry>();
    private PerlinShake _shaker;
}
