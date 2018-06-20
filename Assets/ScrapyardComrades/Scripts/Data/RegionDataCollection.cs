using UnityEngine;
using System.Collections.Generic;

public class RegionDataCollection : ScriptableObject
{
    public List<RegionData> Data;

    [System.Serializable]
    public struct RegionData
    {
        public string RegionName;
        public string[] RoomPrefixes;

        //TODO: ?
        //public PrefabCollection RegionSpecific
        //music
    }
}
