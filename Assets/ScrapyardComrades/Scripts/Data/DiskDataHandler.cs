using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DiskDataHandler
{
    public static void Save(string path, object data)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Exists(fullPath) ? File.Open(fullPath, FileMode.Open) : File.Create(fullPath);
        bf.Serialize(file, data);
        file.Close();
    }

    public static T Load<T>(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        if (File.Exists(fullPath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fullPath, FileMode.Open);
            T data = (T)bf.Deserialize(file);
            file.Close();

            return data;
        }
        return default(T);
    }

    public static bool Erase(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }

    public static bool FileExists(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        return File.Exists(fullPath);
    }

    public static void GuaranteeDirectoryExists(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        DirectoryInfo info = new DirectoryInfo(fullPath);
        if (!info.Exists)
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public static string[] GetAllFilesAtPath(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path;
        DirectoryInfo info = new DirectoryInfo(fullPath);
        if (!info.Exists)
            return new string[0];

        FileInfo[] files = info.GetFiles();
        string[] retVal = new string[files.Length];
        for (int i = 0; i < files.Length; ++i)
        {
            retVal[i] = files[i].Name;
        }

        return retVal;
    }
}
