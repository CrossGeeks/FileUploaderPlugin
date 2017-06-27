using CoreFoundation;
using Foundation;
using Plugin.FileUploader.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.FileUploader
{
  /// <summary>
  /// Implementation for FileUploader
  /// </summary>
    public class FileUploadManager : NSUrlSessionDataDelegate, IFileUploader
    {
        string partBoundary;
        public const string SessionId = "fileuploader";
        public const string UploadFileSuffix = "-multi-part";
        static readonly Encoding encoding = Encoding.UTF8;
        string tag = "";
        public static Action UrlSessionCompletion { get; set; }
        TaskCompletionSource<FileUploadResponse> uploadCompletionSource;
        NSMutableData _data = new NSMutableData();

        public event EventHandler<FileUploadResponse> FileUploadCompleted = delegate { };
        public event EventHandler<FileUploadResponse> FileUploadError = delegate { };
        string multiPartPath = string.Empty;
        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            tag = fileItem.Path;
            string path = fileItem.Path;
            var tmpPath = path;
            var fileName = tmpPath.Substring(tmpPath.LastIndexOf("/") + 1);
            if (path.StartsWith("/var/"))
            {
				var data = NSData.FromUrl(new NSUrl($"file://{path}"));
                tmpPath = SaveToDisk(data, "tmp", fileName);
            }

            multiPartPath = $"{tmpPath}{DateTime.Now.ToString("yyyMMdd_HHmmss")}{UploadFileSuffix}";
     
            partBoundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");


            await SaveToFileSystemAsync(tmpPath, fileItem.FieldName, fileName, multiPartPath, parameters);

            return await MakeRequest(url, headers);
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            tag = fileItem.Name;
            string tmpPath = GetOutputPath("tmp","tmp", fileItem.Name);

            multiPartPath = $"{tmpPath}{DateTime.Now.ToString("yyyMMdd_HHmmss")}{UploadFileSuffix}";

            partBoundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");


            await SaveToFileSystemAsync(fileItem.Bytes, fileItem.FieldName, fileItem.Name, multiPartPath, parameters);

            return await MakeRequest(url, headers);
        }

        async Task SaveToFileSystemAsync(byte[] fileBytes, string fieldName, string fileName, string filePath, IDictionary<string, string> parameters = null)
        {

            await Task.Run(() =>
            {
                // Construct the body
                System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                if (parameters != null)
                {
                    foreach (string vkp in parameters.Keys)
                    {
                        if (parameters[vkp] != null)
                        {
                            sb.AppendFormat("--{0}\r\n", partBoundary);
                            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n", vkp);
                            sb.AppendFormat("{0}\r\n", parameters[vkp]);
                        }
                    }
                }


                sb.AppendFormat("--{0}\r\n", partBoundary);
                sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", fieldName, fileName);
                sb.Append("Content-Type: */*\r\n\r\n");

                // Delete any previous body data file
                if (File.Exists(filePath))
                    File.Delete(filePath);


                using (var writeStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    writeStream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));
                    writeStream.Write(fileBytes, 0, fileBytes.Length);

                    sb.Clear();
                    sb.AppendFormat("\r\n--{0}--\r\n", partBoundary);
                    writeStream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));
                }
                sb = null;
                fileBytes = null;
            });
        }

        async Task SaveToFileSystemAsync(string fileToUpload, string fieldName, string fileName, string filePath, IDictionary<string, string> parameters = null)
        {

            if (File.Exists(fileToUpload))
            {
                await Task.Run(async() =>
                {
                    // Write file to BodyPart
                    var fileBytes = File.ReadAllBytes(fileToUpload);


                    await SaveToFileSystemAsync(fileBytes, fieldName, fileName, filePath, parameters);

					// Delete temporal file
					if (File.Exists(fileToUpload))
						File.Delete(fileToUpload);
                });



            }
            else
            {
                Console.WriteLine("Upload file doesn't exist. File: {0}", filePath);
            }
        }

        NSUrlSessionConfiguration CreateSessionConfiguration(IDictionary<string, string> headers, string identifier)
        {
            var sessionConfiguration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(identifier);

            var headerDictionary = new NSMutableDictionary();
            headerDictionary.Add(new NSString("Accept"), new NSString("application/json"));
            headerDictionary.Add(new NSString("Content-Type"), new NSString(string.Format("multipart/form-data; boundary={0}", partBoundary)));


            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (!string.IsNullOrEmpty(headers[key]))
                    {
                        var headerKey = new NSString(key);
                        if (headerDictionary.ContainsKey(new NSString(key)))
                        {
                            headerDictionary[headerKey] = new NSString(headers[key]);
                        }
                        else
                        {
                            headerDictionary.Add(new NSString(key), new NSString(headers[key]));
                        }
                        
                    }
                }
            }


            sessionConfiguration.HttpAdditionalHeaders = headerDictionary;
            sessionConfiguration.AllowsCellularAccess = true;

            sessionConfiguration.NetworkServiceType = NSUrlRequestNetworkServiceType.Default;
            sessionConfiguration.TimeoutIntervalForRequest = 30;
            //sessionConfiguration.HttpMaximumConnectionsPerHost=1;
            //sessionConfiguration.Discretionary = true;
            return sessionConfiguration;
        }

        async Task<FileUploadResponse> MakeRequest(string url, IDictionary<string, string> headers)
        {
            var request = new NSMutableUrlRequest(NSUrl.FromString(url));
            request.HttpMethod = "POST";
            request["Accept"] = "*/*";
            request["Content-Type"] = "multipart/form-data; boundary=" + partBoundary;
            uploadCompletionSource = new TaskCompletionSource<FileUploadResponse>();

            var sessionConfiguration = CreateSessionConfiguration(headers, $"{SessionId}{multiPartPath}");

            var session = NSUrlSession.FromConfiguration(sessionConfiguration, (INSUrlSessionDelegate)this, NSOperationQueue.MainQueue);

            var uploadTask = session.CreateUploadTask(request, new NSUrl(multiPartPath, false));

            uploadTask.TaskDescription = multiPartPath;
            uploadTask.Priority = NSUrlSessionTaskPriority.High;
            uploadTask.Resume();


            var retVal = await uploadCompletionSource.Task;

            return retVal;
        }


        public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            Console.WriteLine(string.Format("DidCompleteWithError TaskId: {0}{1}", task.TaskIdentifier, (error == null ? "" : " Error: " + error.Description)));

            NSString dataString = NSString.FromData(_data, NSStringEncoding.UTF8);
            var message = dataString == null ? string.Empty : $"{dataString}";
            var responseError = false;
            NSHttpUrlResponse response = null;
            if (task.Response is NSHttpUrlResponse)
            {
                response = task.Response as NSHttpUrlResponse;
                Console.WriteLine("HTTP Response {0}", response);
                Console.WriteLine("HTTP Status {0}", response.StatusCode);
                responseError = response.StatusCode != 200 && response.StatusCode != 201;
            }

            System.Diagnostics.Debug.WriteLine("COMPLETE");

			//Remove the temporal multipart file
			if (File.Exists(multiPartPath))
			{
				File.Delete(multiPartPath);
			}

            if (error == null && !responseError)
            {
                var fileUploadResponse = new FileUploadResponse(message, (int)response?.StatusCode,tag);
                uploadCompletionSource.SetResult(fileUploadResponse);
                FileUploadCompleted(this, fileUploadResponse);

            }
            else if (responseError)
            {
                var fileUploadResponse = new FileUploadResponse(message, (int)response?.StatusCode,tag);
                uploadCompletionSource.SetResult(fileUploadResponse);
                FileUploadError(this, fileUploadResponse);
            }
            else
            {
                var fileUploadResponse = new FileUploadResponse(error.Description, (int)response?.StatusCode,tag);
                uploadCompletionSource.SetResult(fileUploadResponse);
                FileUploadError(this, fileUploadResponse);
            }

            _data = new NSMutableData();
        }

        public override void DidReceiveData(NSUrlSession session, NSUrlSessionDataTask dataTask, NSData data)
        {
            System.Diagnostics.Debug.WriteLine("DidReceiveData...");
            _data.AppendData(data);
        }

        public override void DidReceiveResponse(NSUrlSession session, NSUrlSessionDataTask dataTask, NSUrlResponse response, Action<NSUrlSessionResponseDisposition> completionHandler)
        {
            System.Diagnostics.Debug.WriteLine("DidReceiveResponse:  " + response.ToString());

            completionHandler.Invoke(NSUrlSessionResponseDisposition.Allow);
        }

        public override void DidBecomeDownloadTask(NSUrlSession session, NSUrlSessionDataTask dataTask, NSUrlSessionDownloadTask downloadTask)
        {
            System.Diagnostics.Debug.WriteLine("DidBecomeDownloadTask");
        }


        public override void DidBecomeInvalid(NSUrlSession session, NSError error)
        {
            System.Diagnostics.Debug.WriteLine("DidBecomeInvalid" + (error == null ? "undefined" : error.Description));
        }


        public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            System.Diagnostics.Debug.WriteLine("DidFinishEventsForBackgroundSession");

            if (UrlSessionCompletion != null)
            {
                var completionHandler = UrlSessionCompletion;

                UrlSessionCompletion = null;

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    completionHandler();
                });
            }


        }

        public override void DidSendBodyData(NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("DidSendBodyData bSent: {0}, totalBSent: {1} totalExpectedToSend: {2}", bytesSent, totalBytesSent, totalBytesExpectedToSend));
        }



		string GetOutputPath(string directoryName, string bundleName, string name)
		{

			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
			Directory.CreateDirectory(path);

			if (string.IsNullOrWhiteSpace(name))
			{
				string timestamp = DateTime.Now.ToString("yyyMMdd_HHmmss");

				name = $"{bundleName}_{timestamp}.jpg";
			}



			return Path.Combine(path, GetUniquePath(path, name));
		}

		string GetUniquePath(string path, string name)
		{

			string ext = Path.GetExtension(name);
			if (ext == String.Empty)
				ext = ".jpg";

			name = Path.GetFileNameWithoutExtension(name);

			string nname = name + ext;
			int i = 1;
			while (File.Exists(Path.Combine(path, nname)))
				nname = name + "_" + (i++) + ext;


			return Path.Combine(path, nname);


		}

        string SaveToDisk(NSData data, string bundleName, string fileName = null, string directoryName = null)
		{


			NSError err = null;
			string path = GetOutputPath(directoryName ?? bundleName, bundleName, fileName);

			if (!File.Exists(path))
			{
               
                    if (data.Save(path, true, out err))
                    {
                        System.Diagnostics.Debug.WriteLine("saved as " + path);
                        Console.WriteLine("saved as " + path);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("NOT saved as " + path +
                            " because" + err.LocalizedDescription);
                    }
               
			}

			return path;
		}
    }

}