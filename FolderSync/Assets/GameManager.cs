using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        var folderData = FolderManager.LoadFolderData();
        if(folderData != null)
        {
            Debug.Log("Loaded folder: " + folderData);
        }
        else
        {
            Debug.LogWarning("No folder data found");
        }
    }


    public void SelectFolder()
    {
        var localPath = FolderManager.SelectFolder();
        if(localPath == null) return;

        var folderData = new FolderData(localPath, System.Guid.NewGuid().ToString());
        FolderManager.SaveFolderData(folderData);
    }
}
