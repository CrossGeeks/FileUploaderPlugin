using CoreFoundation;
using Foundation;
#if __MAC__
#else
using MobileCoreServices;
#endif
using Plugin.FileUploader.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FileUploader
{
  /// <summary>
  /// Implementation for FileUploader
  /// </summary>
    public class FileUploadManager : NSUrlSessionDataDelegate, IFileUploader
    {
#if __MAC__
        IDictionary<string, string> mimeTypes = new Dictionary<string, string>()
        {
              {"html", "text/html"},
              {"htm", "text/html"},
              {"shtml", "text/html"},
              {"css", "text/css"},
              {"xml", "text/xml"},
              {"gif", "image/gif"},
              {"jpeg", "image/jpeg"},
              {"jpg", "image/jpeg"},
              {"js", "application/javascript"},
              {"atom", "application/atom+xml"},
              {"rss", "application/rss+xml"},
              {"mml", "text/mathml"},
              {"txt", "text/plain"},
              {"jad", "text/vnd.sun.j2me.app-descriptor"},
              {"wml", "text/vnd.wap.wml"},
              {"htc", "text/x-component"},
              {"png", "image/png"},
              {"tif", "image/tiff"},
              {"tiff", "image/tiff"},
              {"wbmp", "image/vnd.wap.wbmp"},
              {"ico", "image/x-icon"},
              {"jng", "image/x-jng"},
              {"bmp", "image/x-ms-bmp"},
              {"svg", "image/svg+xml"},
              {"svgz", "image/svg+xml"},
              {"webp", "image/webp"},
              {"woff", "application/font-woff"},
              {"jar", "application/java-archive"},
              {"war", "application/java-archive"},
              {"ear", "application/java-archive"},
              {"json", "application/json"},
              {"hqx", "application/mac-binhex40"},
              {"doc", "application/msword"},
              {"pdf", "application/pdf"},
              {"ps", "application/postscript"},
              {"eps", "application/postscript"},
              {"ai", "application/postscript"},
              {"rtf", "application/rtf"},
              {"m3u8", "application/vnd.apple.mpegurl"},
              {"xls", "application/vnd.ms-excel"},
              {"eot", "application/vnd.ms-fontobject"},
              {"ppt", "application/vnd.ms-powerpoint"},
              {"wmlc", "application/vnd.wap.wmlc"},
              {"kml", "application/vnd.google-earth.kml+xml"},
              {"kmz", "application/vnd.google-earth.kmz"},
              {"7z", "application/x-7z-compressed"},
              {"cco", "application/x-cocoa"},
              {"jardiff", "application/x-java-archive-diff"},
              {"jnlp", "application/x-java-jnlp-file"},
              {"run", "application/x-makeself"},
              {"pl", "application/x-perl"},
              {"pm", "application/x-perl"},
              {"prc", "application/x-pilot"},
              {"pdb", "application/x-pilot"},
              {"rar", "application/x-rar-compressed"},
              {"rpm", "application/x-redhat-package-manager"},
              {"sea", "application/x-sea"},
              {"swf", "application/x-shockwave-flash"},
              {"sit", "application/x-stuffit"},
              {"tcl", "application/x-tcl"},
              {"tk", "application/x-tcl"},
              {"der", "application/x-x509-ca-cert"},
              {"pem", "application/x-x509-ca-cert"},
              {"crt", "application/x-x509-ca-cert"},
              {"xpi", "application/x-xpinstall"},
              {"xhtml", "application/xhtml+xml"},
              {"xspf", "application/xspf+xml"},
              {"zip", "application/zip"},
              {"bin", "application/octet-stream"},
              {"exe", "application/octet-stream"},
              {"dll", "application/octet-stream"},
              {"deb", "application/octet-stream"},
              {"dmg", "application/octet-stream"},
              {"iso", "application/octet-stream"},
              {"img", "application/octet-stream"},
              {"msi", "application/octet-stream"},
              {"msp", "application/octet-stream"},
              {"msm", "application/octet-stream"},
              {"docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
              {"xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
              {"pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
              {"mid", "audio/midi"},
              {"midi", "audio/midi"},
              {"kar", "audio/midi"},
              {"mp3", "audio/mpeg"},
              {"ogg", "audio/ogg"},
              {"m4a", "audio/x-m4a"},
              {"ra", "audio/x-realaudio"},
              {"3gpp", "video/3gpp"},
              {"3gp", "video/3gpp"},
              {"ts", "video/mp2t"},
              {"mp4", "video/mp4"},
              {"mpeg", "video/mpeg"},
              {"mpg", "video/mpeg"},
              {"mov", "video/quicktime"},
              {"webm", "video/webm"},
              {"flv", "video/x-flv"},
              {"m4v", "video/x-m4v"},
              {"mng", "video/x-mng"},
              {"asx", "video/x-ms-asf"},
              {"asf", "video/x-ms-asf"},
              {"wmv", "video/x-ms-wmv"},
              {"avi", "video/x-msvideo"}
        };
        #endif
        public const string SessionId = "fileuploader";
        public const string UploadFileSuffix = "-multi-part";
        static readonly Encoding encoding = Encoding.UTF8;
        public static Action UrlSessionCompletion { get; set; }
        TaskCompletionSource<FileUploadResponse> uploadCompletionSource;
       // NSMutableData _data = new NSMutableData();
        IDictionary<nuint, NSMutableData> uploadData = new Dictionary<nuint, NSMutableData>();
        public event EventHandler<FileUploadResponse> FileUploadCompleted = delegate { };
        public event EventHandler<FileUploadResponse> FileUploadError = delegate { };
        public event EventHandler<FileUploadProgress> FileUploadProgress = delegate { };

        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null,string boundary = null)
        {
            return await UploadFileAsync(url, new FilePathItem[] { fileItem },fileItem.Path, headers, parameters,boundary);
        }
        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem[] fileItems,string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null)
        {
            if (fileItems == null || fileItems.Length == 0)
            {
                var fileUploadResponse = new FileUploadResponse("There are no items to upload", -1, tag);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }

            bool error = false;
            string errorMessage = string.Empty;

            var uploadItems = new List<UploadFileItemInfo>();
            foreach(var fileItem in fileItems)
            {
                bool temporal = false;
                string path = fileItem.Path;
                var tmpPath = path;
                var fileName = tmpPath.Substring(tmpPath.LastIndexOf("/") + 1);
                if (path.StartsWith("/var/"))
                {
                    var data = NSData.FromUrl(new NSUrl($"file://{path}"));
                    tmpPath = SaveToDisk(data, "tmp", fileName);
                    temporal = true;
                }

                if(string.IsNullOrEmpty(boundary))
                {
                    boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                }

                if (File.Exists(tmpPath))
                {
                    uploadItems.Add(new UploadFileItemInfo(tmpPath, fileItem.FieldName, fileName, temporal));
                }
                else
                {
                    error = true;
                    errorMessage = $"File at path: {fileItem.Path} doesn't exist";
                    break;
                }

            }

            if(error)
            {
                var fileUploadResponse = new FileUploadResponse(errorMessage, -1, tag);
                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }

            var mPath = await SaveToFileSystemAsync(uploadItems.ToArray(), parameters,boundary);
          


            return await MakeRequest(mPath, tag, url, headers,boundary);
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null,string boundary = null)
        {
            if (string.IsNullOrEmpty(boundary))
            {
                boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            }


            var mPath = await SaveToFileSystemAsync(new UploadFileItemInfo[] { new UploadFileItemInfo(fileItem.Bytes, fileItem.FieldName, fileItem.Name) }, parameters,boundary);

            return await MakeRequest(mPath, fileItem.Name, url, headers,boundary);
        }
        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem[] fileItems, string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null,string boundary =null)
        {

            if (string.IsNullOrEmpty(boundary))
            {
                boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            }
            
            var uploadItems = new List<UploadFileItemInfo>();
            foreach (var fileItem in fileItems)
            {

                uploadItems.Add(new UploadFileItemInfo(fileItem.Bytes, fileItem.FieldName, fileItem.Name));

            }

            var mPath = await SaveToFileSystemAsync(uploadItems.ToArray(), parameters,boundary);

            return await MakeRequest(mPath, tag, url, headers,boundary);
        }
       
        async Task<string> SaveToFileSystemAsync(UploadFileItemInfo[] itemsToUpload, IDictionary<string, string> parameters = null,string boundary = null)
        {
           return await Task.Run(() =>
            {
                // Construct the body
                System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                if (parameters != null)
                {
                    foreach (string vkp in parameters.Keys)
                    {
                        if (parameters[vkp] != null)
                        {
                            sb.AppendFormat("--{0}\r\n", boundary);
                            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n", vkp);
                            sb.AppendFormat("{0}\r\n", parameters[vkp]);
                        }
                    }
                }


         


                string tmpPath = GetOutputPath("tmp", "tmp", null);
                var multiPartPath = $"{tmpPath}{DateTime.Now.ToString("yyyMMdd_HHmmss")}{UploadFileSuffix}";


                // Delete any previous body data file
                if (File.Exists(multiPartPath))
                        File.Delete(multiPartPath);


                    using (var writeStream = new FileStream(multiPartPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        writeStream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));

                        foreach (var fileInfo in itemsToUpload)
                        {
                            sb.Clear();
                            sb.AppendFormat("--{0}\r\n", boundary);
                            sb.Append($"Content-Disposition: form-data; name=\"{fileInfo.FieldName}\"; filename=\"{fileInfo.FileName}\"\r\n");
                            sb.Append($"Content-Type: {GetMimeType(fileInfo.FileName)}\r\n\r\n");

                            writeStream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));
                            if (fileInfo.Data != null)
                            {
                                writeStream.Write(fileInfo.Data, 0, fileInfo.Data.Length);

                            } else if (!string.IsNullOrEmpty(fileInfo.OriginalPath) && File.Exists(fileInfo.OriginalPath))
                            {
                                var data = File.ReadAllBytes(fileInfo.OriginalPath);
                                writeStream.Write(data, 0, data.Length);
                            }
                            
                        writeStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                            //delete temporal files created
                            if (fileInfo.IsTemporal && File.Exists(fileInfo.OriginalPath))
                            {
                                File.Delete(fileInfo.OriginalPath);
                            }
                        
                           
                            fileInfo.Data = null;
                        }
                    var pBoundary = $"\r\n--{boundary}--\r\n";
                    writeStream.Write(encoding.GetBytes(pBoundary), 0, encoding.GetByteCount(pBoundary));
                }
   

                sb = null;
                return multiPartPath;
            });
        }

        NSUrlSessionConfiguration CreateSessionConfiguration(IDictionary<string, string> headers, string identifier,string boundary)
        {
            var sessionConfiguration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(identifier);

            var headerDictionary = new NSMutableDictionary();
            headerDictionary.Add(new NSString("Accept"), new NSString("application/json"));
            headerDictionary.Add(new NSString("Content-Type"), new NSString(string.Format("multipart/form-data; boundary={0}", boundary)));


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

        async Task<FileUploadResponse> MakeRequest(string uploadPath,string tag, string url, IDictionary<string, string> headers,string boundary)
        {
            var request = new NSMutableUrlRequest(NSUrl.FromString(url));
            request.HttpMethod = "POST";
            request["Accept"] = "*/*";
            request["Content-Type"] = "multipart/form-data; boundary=" + boundary;
            uploadCompletionSource = new TaskCompletionSource<FileUploadResponse>();

            var sessionConfiguration = CreateSessionConfiguration(headers, $"{SessionId}{uploadPath}",boundary);

            var session = NSUrlSession.FromConfiguration(sessionConfiguration, (INSUrlSessionDelegate)this, NSOperationQueue.MainQueue);

            var uploadTask = session.CreateUploadTask(request, new NSUrl(uploadPath, false));
        
            uploadTask.TaskDescription = $"{tag}|{uploadPath}";
            uploadTask.Priority = NSUrlSessionTaskPriority.High;
            uploadTask.Resume();


            var retVal = await uploadCompletionSource.Task;

            return retVal;
        }


        public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            Console.WriteLine(string.Format("DidCompleteWithError TaskId: {0}{1}", task.TaskIdentifier, (error == null ? "" : " Error: " + error.Description)));
            NSMutableData _data = null;

            if (uploadData.ContainsKey(task.TaskIdentifier))
            {
                _data = uploadData[task.TaskIdentifier];
                uploadData.Remove(task.TaskIdentifier);
            }
            else
            {
                _data = new NSMutableData();
            }

            NSString dataString = NSString.FromData(_data, NSStringEncoding.UTF8);
            var message = dataString == null ? string.Empty : $"{dataString}";
            var responseError = false;
            NSHttpUrlResponse response = null;

            string[] parts=task.TaskDescription.Split('|');

            if (task.Response is NSHttpUrlResponse)
            {
                response = task.Response as NSHttpUrlResponse;
                Console.WriteLine("HTTP Response {0}", response);
                Console.WriteLine("HTTP Status {0}", response.StatusCode);
                responseError = response.StatusCode != 200 && response.StatusCode != 201;
            }

            System.Diagnostics.Debug.WriteLine("COMPLETE");

			//Remove the temporal multipart file
			if (parts != null && parts.Length > 0 && File.Exists(parts[1]))
			{
				File.Delete(parts[1]);
            }

            if (parts == null || parts.Length == 0)
                parts = new string[] { string.Empty, string.Empty };


            if (error == null && !responseError)
            {
                var fileUploadResponse = new FileUploadResponse(message, (int)response?.StatusCode, parts[0]);
                uploadCompletionSource.TrySetResult(fileUploadResponse);
                FileUploadCompleted(this, fileUploadResponse);

            }
            else if (responseError)
            {
                var fileUploadResponse = new FileUploadResponse(message, (int)response?.StatusCode, parts[0]);
                uploadCompletionSource.TrySetResult(fileUploadResponse);
                FileUploadError(this, fileUploadResponse);
            }
            else
            {
                var fileUploadResponse = new FileUploadResponse(error.Description, (int)response?.StatusCode, parts[0]);
                uploadCompletionSource.TrySetResult(fileUploadResponse);
                FileUploadError(this, fileUploadResponse);
            }
          
            _data = null;
        }

        public override void DidReceiveData(NSUrlSession session, NSUrlSessionDataTask dataTask, NSData data)
        {
            System.Diagnostics.Debug.WriteLine("DidReceiveData...");
            if(uploadData.ContainsKey(dataTask.TaskIdentifier))
            {
                uploadData[dataTask.TaskIdentifier].AppendData(data);
            }
            else
            {
                var uData = new NSMutableData();
                uData.AppendData(data);
                uploadData.Add(dataTask.TaskIdentifier,uData);
            }
           // _data.AppendData(data);
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
            string[] parts = task.TaskDescription.Split('|');

            var tag = string.Empty;

            if (parts != null && parts.Length > 0)
            {
                tag = parts[0];
            }

            var fileUploadProgress = new FileUploadProgress(totalBytesSent,totalBytesExpectedToSend,tag);
            FileUploadProgress(this, fileUploadProgress);

            System.Diagnostics.Debug.WriteLine(string.Format("DidSendBodyData bSent: {0}, totalBSent: {1} totalExpectedToSend: {2}", bytesSent, totalBytesSent, totalBytesExpectedToSend));
        }



		string GetOutputPath(string directoryName, string bundleName, string name)
		{
#if __MAC__
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directoryName);
#else
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
#endif
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

        public string GetMimeType(string fileName)
        {
#if __MAC__
            try
            {
                var extensionWithDot = Path.GetExtension(fileName);
                if (!string.IsNullOrWhiteSpace(extensionWithDot))
                {
                    var extension = extensionWithDot.Substring(1);
                    if (!string.IsNullOrWhiteSpace(extension)&&mimeTypes.ContainsKey(extension))
                    {
                       return mimeTypes[extension];
                    }
                }
            }catch(Exception ex)
            {

            }
#else
                        try
                        {
                            var extensionWithDot = Path.GetExtension(fileName);
                            if (!string.IsNullOrWhiteSpace(extensionWithDot))
                            {
                                var extension = extensionWithDot.Substring(1);
                                if (!string.IsNullOrWhiteSpace(extension))
                                {
                                    var extensionClassRef = new NSString(UTType.TagClassFilenameExtension);
                                    var mimeTypeClassRef = new NSString(UTType.TagClassMIMEType);

                                    var uti = NativeTools.UTTypeCreatePreferredIdentifierForTag(extensionClassRef.Handle, new NSString(extension).Handle, IntPtr.Zero);
                                    var mimeType = NativeTools.UTTypeCopyPreferredTagWithClass(uti, mimeTypeClassRef.Handle);
                                    using (var mimeTypeCString = new CoreFoundation.CFString(mimeType))
                                    {
                                        return mimeTypeCString;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
#endif


                        return "*/*";
        }
        class UploadFileItemInfo
        {
            public byte[] Data { get; set; }
            public string FieldName { get;  }
            public string FileName { get; }
            public string OriginalPath { get;}

            public bool IsTemporal { get; }

            public UploadFileItemInfo(byte[] data,string fieldName,string fileName)
            {
                Data = data;
                FieldName = fieldName;
                FileName = fileName;
            }

            public UploadFileItemInfo(string originalPath, string fieldName, string fileName,bool isTemporal)
            {
                OriginalPath = originalPath;
                FieldName = fieldName;
                FileName = fileName;
                IsTemporal = isTemporal;
            }
        }

    }

}