using System;
using UnityEngine;
using System.IO;
public class FolderData
{
    public string folderPath;
    public string id;
    public string lastModified;

    public DateTime LastModified
    {
        get { return DateTime.Parse(lastModified); }
        set { lastModified = value.ToString("yyyy-MM-ddTHH:mm:ss"); }
    }

    public FolderData(string folderPath, string id)
    {
        this.folderPath = folderPath;
        this.id = id;
        LastModified = DateTime.Now;
    }

    public override string ToString()
    {
        return string.Format("FolderData: {0}, {1}, {2}", folderPath, id, lastModified);
    }
}
public static class FolderManager 
{
    private const string saveFileName = "folderData.json";

    public static void SaveFolderData(FolderData folderData)
    {
        string json = JsonUtility.ToJson(folderData);
        System.IO.File.WriteAllText(saveFileName, json);
    }

    public static FolderData LoadFolderData()
    {
        if (System.IO.File.Exists(saveFileName))
        {
            string json = System.IO.File.ReadAllText(saveFileName);
            return JsonUtility.FromJson<FolderData>(json);
        }
        else
        {
            return null;
        }
    }

    public static void DeleteFolderData()
    {
        if (System.IO.File.Exists(saveFileName))
        {
            System.IO.File.Delete(saveFileName);
        }
    }

    //public static string SelectFolder()
    //{
    //    //string folderPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Folder", "", "");

    //    if (!string.IsNullOrEmpty(folderPath))
    //    {
    //        Debug.Log("Selected folder: " + folderPath);

    //        return folderPath;
    //        //FolderData folderData = new FolderData(folderPath, System.Guid.NewGuid().ToString());
    //        //SaveFolderData(folderData);
    //    }
    //    else
    //    {
    //        Debug.Log("Folder selection cancelled.");
    //        return null;
    //    }
    //}
}
