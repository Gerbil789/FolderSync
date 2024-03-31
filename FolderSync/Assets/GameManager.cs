using UnityEngine;
using GoogleDrive;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        var service = await Drive.InitializeDriveService(Application.dataPath + "/credentials.json");
        if(service == null)
        {
            Debug.LogError("Failed to initialize drive service");
            return;
        }

        var folderId = await Drive.GetFolderId(service, "Test");
        Debug.Log("Folder ID: " + folderId);

    }


    public void SelectFolder()
    {
        var localPath = FolderManager.SelectFolder();
        if(localPath == null) return;

        var folderData = new FolderData(localPath, System.Guid.NewGuid().ToString());
        FolderManager.SaveFolderData(folderData);
    }
}
