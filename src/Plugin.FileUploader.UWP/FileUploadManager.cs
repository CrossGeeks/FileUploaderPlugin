using Plugin.FileUploader.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.FileUploader
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class FileUploadManager : IFileUploader
    {
        public event EventHandler<FileUploadResponse> FileUploadCompleted;
        public event EventHandler<FileUploadResponse> FileUploadError;

        public Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, string token = null, IDictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem[] fileItems, string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem[] fileItems, string tag, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }
    }
}