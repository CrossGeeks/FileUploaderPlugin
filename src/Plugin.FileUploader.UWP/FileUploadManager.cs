using Plugin.FileUploader.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

namespace Plugin.FileUploader
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class FileUploadManager : IFileUploader
    {
        CancellationTokenSource cts;
        public FileUploadManager()
        {
            DiscoverActiveUploadsAsync();
            cts = new CancellationTokenSource();
        }
        public event EventHandler<FileUploadResponse> FileUploadCompleted = delegate { };
        public event EventHandler<FileUploadResponse> FileUploadError = delegate { };
        public event EventHandler<FileUploadProgress> FileUploadProgress = delegate { };

        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null,string boundary = null)
        {
            return await UploadFileAsync(url, new FilePathItem[] { fileItem }, fileItem.Path.Substring(fileItem.Path.LastIndexOf("/") + 1), headers, parameters,boundary);
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem[] fileItems, string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null)
        {
            if (fileItems == null || fileItems.Length == 0)
            {
                var fileUploadResponse = new FileUploadResponse("There are no items to upload", -1, tag, null);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }
            
            Uri uri;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri))
            {
                var fileUploadResponse = new FileUploadResponse("Invalid upload url", -1, tag, null);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }
            BackgroundUploader uploader = new BackgroundUploader();
            var parts=PrepareRequest(uploader,tag,headers,parameters);
            
            for (int i = 0; i < fileItems.Length; i++)
            {
                BackgroundTransferContentPart part = new BackgroundTransferContentPart(fileItems[i].FieldName, fileItems[i].Path.Substring(fileItems[i].Path.LastIndexOf("/") + 1));
                var storageFile = await StorageFile.GetFileFromPathAsync(@fileItems[i].Path); 
                part.SetFile(storageFile);
                parts.Add(part);
            }
            
            UploadOperation upload = null;
            if (string.IsNullOrEmpty(boundary))
            {
                upload = await uploader.CreateUploadAsync(uri, parts);

            }
            else
            {
                upload = await uploader.CreateUploadAsync(uri, parts, "form-data", boundary);
            }

            return await HandleUploadAsync(upload, true);
     
        }
        List<BackgroundTransferContentPart> PrepareRequest(BackgroundUploader uploader,string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            List<BackgroundTransferContentPart> parts = new List<BackgroundTransferContentPart>();

            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (!string.IsNullOrEmpty(headers[key]))
                    {
                        uploader.SetRequestHeader(key, headers[key]);
                    }
                }
            }

            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (parameters[key] != null)
                    {
                        BackgroundTransferContentPart part = new BackgroundTransferContentPart(key);
                        part.SetText(parameters[key]);
                        parts.Add(part);
                    }
                }
            }

            if(tag.Length > 40)
            {
                tag = tag.Substring(0, 40);
            }

            uploader.TransferGroup = BackgroundTransferGroup.CreateGroup(tag);

            return parts;
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null)
        {
            return await UploadFileAsync(url, new FileBytesItem[] { fileItem }, fileItem.Name, headers, parameters,boundary);
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem[] fileItems, string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null)
        {
            if (fileItems == null || fileItems.Length == 0)
            {
                var fileUploadResponse = new FileUploadResponse("There are no items to upload", -1, tag, null);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }

            Uri uri;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri))
            {
                var fileUploadResponse = new FileUploadResponse("Invalid upload url", -1, tag, null);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }
            BackgroundUploader uploader = new BackgroundUploader();
            var parts = PrepareRequest(uploader, tag, headers, parameters);

            for (int i = 0; i < fileItems.Length; i++)
            {
                BackgroundTransferContentPart part = new BackgroundTransferContentPart(fileItems[i].FieldName, fileItems[i].Name);
                var storageFile = await GetStorageFile(tag,fileItems[i].Bytes, fileItems[i].Name);
                part.SetFile(storageFile);
                parts.Add(part);
            }

            UploadOperation upload = null;
            if(string.IsNullOrEmpty(boundary))
            {
                upload = await uploader.CreateUploadAsync(uri, parts);

            }
            else
            {
                upload = await uploader.CreateUploadAsync(uri, parts, "form-data", boundary);
            }

            return await HandleUploadAsync(upload, true);
        }


        async Task<StorageFile> GetStorageFile(string tag,byte[] byteArray, string fileName)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
    
            var tmpFolder = await storageFolder.CreateFolderAsync($"tmp-{tag}", CreationCollisionOption.ReplaceExisting);

            Windows.Storage.StorageFile sampleFile = await tmpFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteBytesAsync(sampleFile, byteArray);
            
            return sampleFile;
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }

            GC.SuppressFinalize(this);
        }
        private async Task<FileUploadResponse> HandleUploadAsync(UploadOperation upload, bool start)
        {
            string response = string.Empty;
            FileUploadResponse fileUploadResponse = null;
            try
            {
                Progress<UploadOperation> progressCallback = new Progress<UploadOperation>(UploadProgress);

                if (start)
                {
                    // Start the upload and attach a progress handler.
                    await upload.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The upload was already running when the application started, re-attach the progress handler.
                    await upload.AttachAsync().AsTask(cts.Token, progressCallback);
                   
                }

               
                using (var inputStream = upload.GetResultStreamAt(0))
                {
                    using (StreamReader tr = new StreamReader(inputStream.AsStreamForRead()))
                    {
                        response = tr.ReadToEnd();

                    }
                }
                ResponseInformation responseInfo = upload.GetResponseInformation();
              
                //Handle this response string.
                fileUploadResponse = new FileUploadResponse(response, (int)responseInfo.StatusCode, upload.TransferGroup.Name, responseInfo.Headers);
                if (responseInfo.StatusCode == 200 || responseInfo.StatusCode == 201)
                {
                    FileUploadCompleted(this, fileUploadResponse);
                }
                else
                {
                    FileUploadError(this, fileUploadResponse);
                }

               

            }
            catch (TaskCanceledException)
            {
                fileUploadResponse = new FileUploadResponse("Upload canceled", -1, upload.TransferGroup.Name,null);
                FileUploadError(this, fileUploadResponse);
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Error", ex, upload))
                {
                    fileUploadResponse = new FileUploadResponse(ex.ToString()+$" * {response}", -1, upload.TransferGroup.Name, null);
                    FileUploadError(this, fileUploadResponse);
                }
            }finally
            {

                //Clear all temporal files
                try
                {
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    var tmpFolder = await storageFolder.GetFolderAsync($"tmp-{upload.TransferGroup.Name}");

                    if (tmpFolder != null)
                    {
                        await tmpFolder.DeleteAsync();
                    }
                }
                catch(System.IO.FileNotFoundException ex)
                {

                }
            }
            
            return fileUploadResponse;
            
        }

        private bool IsExceptionHandled(string title, Exception ex, UploadOperation upload = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (upload == null)
            {
                var fileUploadResponse = new FileUploadResponse(error.ToString(), -1, string.Empty,null);
                FileUploadError(this, fileUploadResponse);
            }
            else
            {
                string response = string.Empty;
                using (var inputStream = upload.GetResultStreamAt(0))
                {
                    using (StreamReader tr = new StreamReader(inputStream.AsStreamForRead()))
                    {
                        response = tr.ReadToEnd();

                    }
                }
                ResponseInformation responseInfo = upload.GetResponseInformation();

                if (string.IsNullOrEmpty(response))
                {
                    response = error.ToString();
                }
                
                var fileUploadResponse = new FileUploadResponse(response,(int) responseInfo.StatusCode, upload.TransferGroup.Name, null);
                FileUploadError(this, fileUploadResponse);
            }

            return true;
        }

        // Note that this event is invoked on a background thread, so we cannot access the UI directly.
        private void UploadProgress(UploadOperation upload)
        {
            // UploadOperation.Progress is updated in real-time while the operation is ongoing. Therefore,
            // we must make a local copy so that we can have a consistent view of that ever-changing state
            // throughout this method's lifetime.
            BackgroundUploadProgress currentProgress = upload.Progress;

            double percentSent = 100;
            if (currentProgress.TotalBytesToSend > 0)
            {
                percentSent = currentProgress.BytesSent * 100 / currentProgress.TotalBytesToSend;
            }

            var fileUploadProgress = new FileUploadProgress((long)currentProgress.BytesSent, (long)currentProgress.TotalBytesToSend, upload.TransferGroup.Name);
            FileUploadProgress(this, fileUploadProgress);

            if (currentProgress.HasRestarted)
            {
               // MarshalLog(" - Upload restarted");
            }

            if (currentProgress.HasResponseChanged)
            {
                // We've received new response headers from the server.

                // If you want to stream the response data this is a good time to start.
                //upload.GetResultStreamAt(0);
      

            }

        }

        // Enumerate the uploads that were going on in the background while the app was closed.
        private async Task DiscoverActiveUploadsAsync()
        {
            IReadOnlyList<UploadOperation> uploads = null;
            try
            {
                uploads = await BackgroundUploader.GetCurrentUploadsAsync();
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Discovery error", ex))
                {
                    var fileUploadResponse = new FileUploadResponse(ex.ToString(), -1, string.Empty, null);
                    FileUploadError(this, fileUploadResponse);
                }
                return;
            }
         

            if (uploads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (UploadOperation upload in uploads)
                {
                    // Attach progress and completion handlers.
                    tasks.Add(HandleUploadAsync(upload, false));
                }

                // Don't await HandleUploadAsync() in the foreach loop since we would attach to the second
                // upload only when the first one completed; attach to the third upload when the second one
                // completes etc. We want to attach to all uploads immediately.
                // If there are actions that need to be taken once uploads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }

        
        
    }
}