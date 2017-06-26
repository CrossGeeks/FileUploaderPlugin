## FileUploader Plugin for Xamarin iOS and Android
Simple cross platform plugin to upload files.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Plugin.FileUploader [![NuGet](https://img.shields.io/nuget/v/Plugin.FileUploader.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.FileUploader/)
* Install into your PCL project and Client projects.

Build Status: 
* [![Build status](https://ci.appveyor.com/api/projects/status/k6l4x6ovp5ysfbar?svg=true)](https://ci.appveyor.com/project/rdelrosario/fileuploaderplugin)
* CI NuGet Feed: https://ci.appveyor.com/nuget/fileuploaderplugin

**Platform Support**

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 7+|
|Xamarin.Android|API 10+|

### API Usage

Call **CrossFileUploader.Current** from any project or PCL to gain access to APIs.


**FileItem**
```csharp
/// <summary>
/// Path: File path location.
/// Name: Name of the file.
/// </summary>
public class FileItem
{
    public string Path { get; } 
    public string Name {get; } 
}
```


**UploadFileAsync**
```csharp
/// <summary>
/// Upload file
/// </summary>
/// <param name="url">Url for file uploading</param>
/// <param name="fileItem">File item to be uploaded</param>
/// <param name="token">Authorization token</param>
/// <param name="parameters">Additional parameters for upload request</param>
/// <returns>FileUploadResponse</returns>
Task<FileUploadResponse> UploadFileAsync(string url, FileItem fileItem, string token = null, IDictionary<string, string> parameters = null);
```

Usage sample:
```csharp

  var response = await CrossFileUploader.Current.UploadFileAsync($"<UPLOAD URL HERE>",new FileItem("<FILE NAME HERE>",filePath));

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
  
  CrossFileUploader.Current.UploadFileAsync($"<UPLOAD URL HERE>",new FileItem("<FILE NAME HERE>",filePath));

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
        FileUploaderImplementation.UrlSessionCompletion = completionHandler;
    }
```

Also consider on iOS 9+, your URL must be secured or you have to add the domain to the list of exceptions. See https://github.com/codepath/ios_guides/wiki/App-Transport-Security