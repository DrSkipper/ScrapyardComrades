using UnityEngine;

public class HeroProgressionData : ScriptableObject
{
    public PooledObject[] HeroPrefabs;
    public LevelInfo[] LevelData;

    [System.Serializable]
    public struct LevelInfo
    {
        public int MaxHealthThreshold;
        public int HealthLostPerAttrition;
    }

    public int MaxHeroLevel { get { return this.HeroPrefabs.Length - 1; } }
}
