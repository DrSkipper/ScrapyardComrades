using UnityEngine;

public class HeroProgressionData : ScriptableObject
{
    public PooledObject[] HeroPrefabs;
    public int[] MaxHealthThresholds;

    public int MaxHeroLevel { get { return this.HeroPrefabs.Length - 1; } }
}
