using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace CrmArcheonzero.Services
{
    public class CloudStorageService
    {
        private readonly DriveService? _driveService;
        private readonly string? _folderId;
        private readonly bool _isEnabled;

        public CloudStorageService(string credentialsPath, string folderId)
        {
            try
            {
                if (File.Exists(credentialsPath))
                {
                    var credential = GoogleCredential.FromFile(credentialsPath)
                        .CreateScoped(DriveService.ScopeConstants.DriveFile);

                    _driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "CRM System"
                    });

                    _folderId = folderId;
                    _isEnabled = true;
                }
                else
                {
                    _isEnabled = false;
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "CloudStorageService.Constructor");
                _isEnabled = false;
            }
        }

        public async Task<string?> UploadFileAsync(string filePath, string? fileName = null)
        {
            if (!_isEnabled || _driveService == null || _folderId == null) return null;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = Path.GetFileName(filePath);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new[] { _folderId }
                };

                using var stream = new FileStream(filePath, FileMode.Open);
                var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
                request.Fields = "id";
                var response = await request.UploadAsync();

                if (response.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    return request.ResponseBody.Id;
                }

                return null;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "CloudStorageService.UploadFileAsync");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            if (!_isEnabled || _driveService == null) return false;

            try
            {
                await _driveService.Files.Delete(fileId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "CloudStorageService.DeleteFileAsync");
                return false;
            }
        }
    }
}