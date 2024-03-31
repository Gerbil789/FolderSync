using UnityEngine;
using GoogleDrive;
using System.IO;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        var localFolderPath = @"C:\Users\vojta\Documents\SyncFolder";

        var service = await Drive.InitializeDriveService(Application.dataPath + "/credentials.json");
        if(service == null)
        {
            Debug.LogError("Failed to initialize drive service");
            return;
        }

        var folderId = await Drive.GetFolderId(service, "Test");
        if(folderId == null)
        {
            Debug.LogError("Failed to get folder id");
            return;
        }

        Debug.Log("Folder ID: " + folderId);

        //var url = @"https://drive.google.com/drive/folders/" + folderId;
        //Application.OpenURL(url);

        await Drive.DownloadFiles(service, folderId, localFolderPath);
    }


    public void SelectFolder()
    {
        var localPath = FolderManager.SelectFolder();
        if(localPath == null) return;

        var folderData = new FolderData(localPath, System.Guid.NewGuid().ToString());
        FolderManager.SaveFolderData(folderData);
    }
}
