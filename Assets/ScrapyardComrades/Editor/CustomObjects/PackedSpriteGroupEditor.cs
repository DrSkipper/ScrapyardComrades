using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(PackedSpriteGroup))]
public class PackedSpriteGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Aggregate Atlases"))
        {
            AggregateAtlases((this.target as PackedSpriteGroup));
        }
    }

    public static void AggregateAtlases(PackedSpriteGroup psg)
    {
        string path = AssetDatabase.GetAssetPath(psg);
        string suffix = psg.name + ".asset";
        path = path.Replace(suffix, "");
        Debug.Log("root path = " + path);
        List<string> relativePaths = new List<string>();
        psg.Atlases = GetAtPath<Texture2D>(path, relativePaths);
        psg.AtlasNames = relativePaths.ToArray();
        EditorUtility.SetDirty(psg);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Custom Objects/Packed Sprite Groups/Aggregate All Atlases")]
    public static void AggregateAllAtlases()
    {
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(PackedSpriteGroup)));

        for (int i = 0; i < guids.Length; ++i)
        {
            string guid = guids[i];
            PackedSpriteGroup psg = AssetDatabase.LoadAssetAtPath<PackedSpriteGroup>(AssetDatabase.GUIDToAssetPath(guid));
            AggregateAtlases(psg);
        }

    }
    
    public static T[] GetAtPath<T>(string path, List<string> relativePaths = null) where T:Object
    {
        List<T> list = new List<T>();
        getAllAtPathRecursive<T>(path, "", list, relativePaths);
        return list.ToArray();
    }

    private static void getAllAtPathRecursive<T>(string path, string namePrefix, List<T> list, List<string> relativePaths) where T:Object
    {
        string fullPath = Application.dataPath.Replace("Assets", "") + path;
        Debug.Log("full path = " + fullPath);

        string[] folderEntries = Directory.GetDirectories(fullPath);
        foreach (string folderName in folderEntries)
        {
            int index = folderName.LastIndexOf("/");
            string name = folderName.Substring(index + 1);

            Debug.Log("asset type = folder");
            getAllAtPathRecursive<T>(path + name + "/", name + "/", list, relativePaths);
        }

        string[] fileEntries = Directory.GetFiles(fullPath);
        foreach (string fileName in fileEntries)
        {
            int index = fileName.LastIndexOf("/");
            string name = fileName.Substring(index + 1);
            string localPath = path + name;

            Debug.Log("asset path = " + localPath);
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(localPath);
            Debug.Log("asset type = " + (type == null ? "null" : type.ToString()));

            if (type == typeof(T))
            {
                T t = AssetDatabase.LoadAssetAtPath<T>(localPath);

                if (t != null)
                {
                    list.Add(t);

                    if (relativePaths != null)
                        relativePaths.Add(namePrefix + name);
                }
            }
        }
    }
}