using UnityEngine;

public class WipeDataOnStart : MonoBehaviour
{
    void Start()
    {
        SaveData.WipeLoadedData();
    }
}
