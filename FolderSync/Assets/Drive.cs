using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.WSA;

namespace GoogleDrive
{
    
    //1082463448755-fuqfcrsf2hk2rermvjcda87uuhfbuvef.apps.googleusercontent.com
    public static class Drive
    {
        private static readonly string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
        private  static readonly string credPath = "token.json";

        public static async Task<UserCredential> GetCredentials(string credentialsFilePath)
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, false));
                }
                return credential;
            }
            catch (FileNotFoundException)
            {
                Debug.Log("Token file not found. User not logged in.");
                return null;
            }
        }
        public static Task<DriveService> InitializeDriveService(UserCredential credential)
        {
            try
            {
                DriveService service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential
                });

                return Task.FromResult(service);
            }
            catch (Exception e)
            {
                Debug.LogError("[INITIALIZE DRIVE SERVICE]: " + e.Message);
                return null;
            }
        }
        public static string GetPageToken(DriveService service)
        {
            try
            {
                var res = service.Changes.GetStartPageToken().Execute();
                return res.StartPageTokenValue;
            }
            catch (Exception e)
            {
                Debug.LogError("[GET PAGE TOKEN]: " + e.Message);
                return null;
            }
        }
        public static string FetchChanges(DriveService service, string savedStartPageToken)
        {
            try
            {
                string pageToken = savedStartPageToken;
                while (pageToken != null)
                {
                    var request = service.Changes.List(pageToken);
                    request.Spaces = "drive";
                    //request.RestrictToMyDrive = true;
                    request.IncludeRemoved = true;
                    var changes = request.Execute();
                    foreach (var change in changes.Changes)
                    {
                        // Process change
                        Console.WriteLine("Change found for file: " + change.FileId);
                    }

                    if (changes.NewStartPageToken != null)
                    {
                        // Last page, save this token for the next polling interval
                        savedStartPageToken = changes.NewStartPageToken;
                    }
                    pageToken = changes.NextPageToken;
                }
                return savedStartPageToken;
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    Console.WriteLine("[ERROR][GET CHANGES]: Credential Not found");
                }
                else
                {
                    Console.WriteLine("[ERROR][GET CHANGES]: " + e.Message);
                    throw;
                }
            }
            return null;
        }
        public static async Task DownloadFile(DriveService service, string fileId, string localFilePath)
        {
            try
            {
                var request = service.Files.Get(fileId);
                var stream = new MemoryStream();
                await request.DownloadAsync(stream);
                System.IO.File.WriteAllBytes(localFilePath, stream.ToArray());
                Debug.Log("Downloaded file: " + localFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("[DOWNLOAD FILE]: " + e.Message);
            }
        }
        public static async Task DownloadFiles(DriveService service, string folderId, string localFolderPath)
        {
            try
            {
                var request = service.Files.List();
                request.Q = $"'{folderId}' in parents"; // Filter by parent folder ID
                request.Fields = "files(id, name)";
                var stream = new MemoryStream();
                foreach (var file in request.Execute().Files)
                {
                    await DownloadFile(service, file.Id, localFolderPath + "/" + file.Name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[DOWNLOAD FILES]: " + e.Message);
            }
        }
        public static async Task Upload(DriveService service, string localFilePath, string folderId)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(localFilePath),
                    Parents = new List<string>() { folderId },
                    Properties = new Dictionary<string, string>()
                    {
                        { "modifiedTime", DateTime.Now.ToString() }
                    }
                };

                using (var stream = new FileStream(localFilePath, FileMode.Open))
                {
                    var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                    var response = await request.UploadAsync();

                    if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                        throw response.Exception;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[UPLOAD FILE]: " + e.Message);
            }
        }
        public static async Task Update(DriveService service, string fileId, string localFilePath)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(localFilePath),
                    Properties = new Dictionary<string, string>()
                    {
                        { "modifiedTime", DateTime.Now.ToString() }
                    }
                };

                using (var stream = new FileStream(localFilePath, FileMode.Open))
                {
                    var request = service.Files.Update(fileMetadata, fileId, stream, "application/octet-stream");
                    var response = await request.UploadAsync();

                    if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                        throw response.Exception;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[UPDATE FILE]: " + e.Message);
            }
        }
        public static async Task<string> GetFolderId(DriveService service, string folderName)
        {
            try
            {
                var request = service.Files.List();
                request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}'"; // Filter by folder name
                request.Fields = "files(id)";

                // Execute the request
                var result = await request.ExecuteAsync();

                if (result.Files != null && result.Files.Count > 0)
                {
                    // Return the ID of the first matching folder
                    return result.Files[0].Id;
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("[GET FOLDER ID]: " + e.Message);
                return null;
            }
        }

        public static async Task<long?> GetFolderVersion(DriveService service, string folderId)
        {
            try
            {
                var request = service.Files.Get(folderId);
                request.Fields = "version";
                var result = await request.ExecuteAsync();
                return result?.Version;
            }
            catch (Exception e)
            {
                Debug.LogError("[GET FOLDER VERSION]: " + e.Message);
                return null;
            }
        }
        public static async Task<DateTime> GetLatestModificationDate(DriveService service, string folderId)
        {
            try
            {
                FilesResource.ListRequest request = service.Files.List();
                request.Q = $"'{folderId}' in parents";
                request.Fields = "files(id, name, parents, modifiedTime, trashed, sha1Checksum, createdTime, trashedTime, capabilities, properties)";
                request.PageSize = 100; // Adjust as needed
                request.OrderBy = "modifiedTime desc";

                var response = await request.ExecuteAsync();

                var trashedfiles = response.Files.Where(f => f.Trashed ?? false).ToList();

                if (!string.IsNullOrEmpty(response.NextPageToken))
                {
                    Console.WriteLine("[WARNING][GET LATEST MODIFICATION DATE]: There are more than 100 files in the folder. Only the first 100 files are considered.");
                }

                var latestCreationDate = response.Files.Max(f => f.CreatedTimeDateTimeOffset?.DateTime) ?? DateTime.MinValue;
                var latestModificationDate = response.Files.Max(f => f.ModifiedTimeDateTimeOffset?.DateTime) ?? DateTime.MinValue;
                var latestTrashedDate = response.Files.Max(f => f.TrashedTimeDateTimeOffset?.DateTime) ?? DateTime.MinValue; // <-- [FIX] RETURN NULL ALWAYS!!! 

                var latestChangeTime = DateTime.MinValue;
                if (latestChangeTime < latestCreationDate) latestChangeTime = latestCreationDate;
                if (latestChangeTime < latestModificationDate) latestChangeTime = latestModificationDate;
                if (latestChangeTime < latestTrashedDate) latestChangeTime = latestTrashedDate;

                return latestChangeTime;
            }
            catch (Exception e)
            {
                Debug.LogError("[GET LATEST MODIFICATION DATE]: " + e.Message);
                return DateTime.MinValue;
            }
        }
    }
}