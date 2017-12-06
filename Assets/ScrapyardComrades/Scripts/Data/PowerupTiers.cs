using UnityEngine;

public class PowerupTiers : ScriptableObject
{
    public Tier[] Tiers;

    [System.Serializable]
    public struct Tier
    {
        public float MinVelocityToTrigger;
        public int MinDurationAtVelocity;
        public int DurationBelowVelocityToDowngrade;
        public PowerupState State;
    }
}
