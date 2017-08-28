using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.FileUploader.Abstractions
{
  /// <summary>
  /// Interface for FileUploader
  /// </summary>
  public interface IFileUploader
  {
        /// <summary>
        /// Event handler when file is upload completes succesfully
        /// </summary>
        event EventHandler<FileUploadResponse> FileUploadCompleted;
        /// <summary>
        /// Event handler when file is upload fails
        /// </summary>
        event EventHandler<FileUploadResponse> FileUploadError;
        /// <summary>
        /// Event handler when file upload is in progress, indicates what's the upload progress so far
        /// </summary>
        event EventHandler<FileUploadProgress> FileUploadProgress;

        /// <summary>
        /// Upload file using file path
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItem">File path item to be uploaded</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <param name="boundary">Custom part boundary</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string,string> headers =null,IDictionary < string, string> parameters = null, string boundary = null);

        /// <summary>
        /// Upload file using file path
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItems">File path items to be uploaded</param>
        /// <param name="tag">Tag reference of the upload request</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <param name="boundary">Custom part boundary</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem[] fileItems,string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null);

        /// <summary>
        /// Upload file using file bytes
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItem">File bytes item to be uploaded</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <param name="boundary">Custom part boundary</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null, string boundary = null);


        /// <summary>
        /// Upload file using file bytes
        /// </summary>
        /// <param name="url">Url for file uploading</param>
        /// <param name="fileItems">File bytes of items to be uploaded</param>
        /// <param name="tag">Tag reference of upload request</param>
        /// <param name="headers">Request headers</param>
        /// <param name="parameters">Additional parameters for upload request</param>
        /// <param name="boundary">Custom part boundary</param>
        /// <returns>FileUploadResponse</returns>
        Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem[] fileItems,string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null,string boundary = null);
    }
}
