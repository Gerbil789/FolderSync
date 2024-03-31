using UnityEngine;
using GoogleDrive;
using System.IO;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        //var localFolderPath = @"C:\Users\vojta\Documents\SyncFolder";

        //var credentials = await Drive.GetCredentials(Application.dataPath + "/credentials.json");
        //if(credentials == null)
        //{
        //    Debug.LogError("Failed to get credentials");
        //    return;
        //}


        //var service = await Drive.InitializeDriveService(credentials);
        //if(service == null)
        //{
        //    Debug.LogError("Failed to initialize drive service");
        //    return;
        //}

        //var folderId = await Drive.GetFolderId(service, "Test");
        //if(folderId == null)
        //{
        //    Debug.LogError("Failed to get folder id");
        //    return;
        //}

        //Debug.Log("Folder ID: " + folderId);

        //var version = await Drive.GetFolderVersion(service, folderId);
        //Debug.Log("Folder version: " + version);

        //var url = @"https://drive.google.com/drive/folders/" + folderId;
        //Application.OpenURL(url);

        //await Drive.DownloadFiles(service, folderId, localFolderPath);
    }
}
