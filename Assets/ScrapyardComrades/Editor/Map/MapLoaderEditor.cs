using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapLoader))]
public class MapLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapLoader mapLoader = this.target as MapLoader;

        if (GUILayout.Button("Load Map"))
        {
            mapLoader.LoadMap();
        }

        if (GUILayout.Button("Correct Tiling"))
        {
            mapLoader.CorrectTiling(true);
        }

        if (GUILayout.Button("Clear Map"))
        {
            mapLoader.ClearMap(true);
        }
    }
}
