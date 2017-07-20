## FileUploader Plugin for Xamarin iOS and Android
Simple cross platform plugin for file multipart uploads.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Plugin.FileUploader [![NuGet](https://img.shields.io/nuget/v/Plugin.FileUploader.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.FileUploader/)
* Install into your PCL project and Client projects.


**Platform Support**

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 7+|
|Xamarin.Android|API 15+|

### API Usage

Call **CrossFileUploader.Current** from any project or PCL to gain access to APIs.

You can upload a file using the file path or bytes.


**FilePathItem**
```csharp
/// <summary>
/// Path: File path location.
/// FieldName: Request field name for the file to be uploaded
/// </summary>
public class FilePathItem
{
    public string Path { get; } 
    public string FieldName {get; } 
}
```


**FileBytesItem**
```csharp
/// <summary>
/// FieldName: Request field name for the file to be uploaded
/// Bytes: File bytes.
/// Name: Name of the file.
/// </summary>
public class FileBytesItem
{
    public string Name { get; }
    public string FieldName { get; }
    public byte[] Bytes { get; }
}
```


**UploadFileAsync**
```csharp
        /// <summary>
        /// Upload file using file path
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItem">File path item to be uploaded</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string,string> headers =null,IDictionary < string, string> parameters = null);

        /// <summary>
        /// Upload file using file bytes
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItem">File bytes item to be uploaded</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null);
```

Usage sample:

Uploading from a file path
```csharp

    CrossFileUploader.Current.UploadFileAsync("<URL HERE>", new FilePathItem("<REQUEST FIELD NAME HERE>","<FILE PATH HERE>"), new Dictionary<string, string>()
                {
                   {"<HEADER KEY HERE>" , "<HEADER VALUE HERE>"}
                }
    );

```

Uploading from a file bytes
```csharp

  CrossFileUploader.Current.UploadFileAsync("<URL HERE>", new FileBytesItem("<REQUEST FIELD NAME HERE>","<FILE BYTES HERE>","<FILE NAME HERE>"), new Dictionary<string, string>()
                {
                   {"<HEADER KEY HERE>" , "<HEADER VALUE HERE>"}
                }
  );

```
Uploading multiple files at once
```csharp

 CrossFileUploader.Current.UploadFileAsync("<URL HERE>", new FilePathItem[]{
    new FilePathItem("file",path1),
	new FilePathItem("file",path2),
	new FilePathItem("file",path3)
 },"Upload Tag 1");

 ```
#### Events in FileUploader
When any file upload completed/failed you can register for an event to fire:
```csharp
/// <summary>
/// Event handler when file is upload completes succesfully
/// </summary>
event EventHandler<FileUploadResponse> FileUploadCompleted; 
```

```csharp
/// <summary>
/// Event handler when file is upload fails
/// </summary>
event EventHandler<FileUploadResponse> FileUploadError; 
```

```csharp
 /// <summary>
 /// Event handler when file upload is in progress, indicates what's the upload progress so far
 /// </summary>
 event EventHandler<FileUploadProgress> FileUploadProgress;
 ```

You will get a FileUploadResponse with the status and response message:
```csharp
public class FileUploadResponse
{
        public string Message { get; }
        public int StatusCode { get; }
}
```

Usage sample:
```csharp

  CrossFileUploader.Current.FileUploadCompleted += (sender, response) =>
  {
    System.Diagnostics.Debug.WriteLine($"{response.StatusCode} - {response.Message}");
  };
  
  CrossFileUploader.Current.UploadFileAsync($"<UPLOAD URL HERE>",new FileItem("<FIELD NAME HERE>","<FILE PATH HERE>"));

```

While upload is in progress you can get feedback on event FileUploadProgress

You will get a FileUploadProgress with the total bytes sent, total request byte length and progress percentage

```csharp
public class FileUploadProgress
{
        public long TotalBytesSent { get; }
        public long TotalLength { get; }
        public double Percentage { get; }

}
```

Usage sample:
```csharp
  CrossFileUploader.Current.FileUploadProgress += (sender, uploadProgress) =>
  {
      System.Diagnostics.Debug.WriteLine($"{response.TotalBytesSent} - {response.Percentage}");
  };
```
### **IMPORTANT**

### iOS:
On AppDelegate.cs

```csharp
    /**
     * Save the completion-handler we get when the app opens from the background.
     * This method informs iOS that the app has finished all internal processing and can sleep again.
     */
    public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
    {
        FileUploadManager.UrlSessionCompletion = completionHandler;
    }
```

Also consider on iOS 9+, your URL must be secured or you have to add the domain to the list of exceptions. See https://github.com/codepath/ios_guides/wiki/App-Transport-Security