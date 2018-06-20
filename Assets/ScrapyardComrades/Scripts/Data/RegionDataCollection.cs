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
    
    public int RegionIndexForRoom(string roomName)
    {
        RegionData data;
        for (int r = 0; r < this.Data.Count; ++r)
        {
            data = this.Data[r];
            for (int i = 0; i < data.RoomPrefixes.Length; ++i)
            {
                if (roomName.Contains(data.RoomPrefixes[i]))
                    return r;
            }
        }

        Debug.LogWarning("Couldn't find region for room: " + roomName + ", assuming region 0");
        return 0;
    }
    
    public bool RegionContainsRoom(int regionIndex, string roomName)
    {
        RegionData data = this.Data[regionIndex];
        for (int i = 0; i < data.RoomPrefixes.Length; ++i)
        {
            if (roomName.Contains(data.RoomPrefixes[i]))
                return true;
        }
        return false;
    }
}
