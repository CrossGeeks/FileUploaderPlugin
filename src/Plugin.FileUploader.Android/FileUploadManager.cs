using Java.Util.Concurrent;
using Newtonsoft.Json.Linq;
using Plugin.FileUploader.Abstractions;
using OkHttp;
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
        public event EventHandler<FileUploadResponse> FileUploadCompleted = delegate { };
        public event EventHandler<FileUploadResponse> FileUploadError = delegate { };
        public async Task<FileUploadResponse> UploadFileAsync(string url, FileBytesItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var requestBodyBuilder = PrepareRequest(parameters);

                    RequestBody fileBody = RequestBody.Create(MediaType.Parse("*/*"), fileItem.Bytes);
                    requestBodyBuilder.AddFormDataPart(fileItem.FieldName, fileItem.Name, fileBody);
                    return MakeRequest(url, requestBodyBuilder, headers);
                }
                catch (Java.Net.UnknownHostException ex)
                {
                    var fileUploadResponse = new FileUploadResponse("Host not reachable", -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }
                catch (Java.IO.IOException ex)
                {
                    var fileUploadResponse = new FileUploadResponse(ex.ToString(), -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }
                catch (Exception ex)
                {
                    var fileUploadResponse = new FileUploadResponse(ex.ToString(), -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }


            });
        }

        public async Task<FileUploadResponse> UploadFileAsync(string url, FilePathItem fileItem, IDictionary<string, string> headers = null, IDictionary<string, string> parameters = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    
                    var requestBodyBuilder = PrepareRequest(parameters);
                    Java.IO.File f = new Java.IO.File(fileItem.Path);
                    string fileAbsolutePath = f.AbsolutePath;

                    RequestBody file_body = RequestBody.Create(MediaType.Parse("*/*"), f);
                    var fileName = fileAbsolutePath.Substring(fileAbsolutePath.LastIndexOf("/") + 1);
                    requestBodyBuilder.AddFormDataPart(fileItem.FieldName,fileName, file_body);

                    return MakeRequest(url,requestBodyBuilder,headers);
                }
                catch (Java.Net.UnknownHostException ex)
                {
                    var fileUploadResponse = new FileUploadResponse("Host not reachable", -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }
                catch (Java.IO.IOException ex)
                {
                    var fileUploadResponse = new FileUploadResponse(ex.ToString(), -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }
                catch (Exception ex)
                {
                    var fileUploadResponse = new FileUploadResponse(ex.ToString(), -1);
                    FileUploadError(this, fileUploadResponse);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return fileUploadResponse;
                }
            });
        }


        MultipartBuilder PrepareRequest(IDictionary<string, string> parameters = null)
        {
            MultipartBuilder requestBodyBuilder = new MultipartBuilder()
                        .Type(MultipartBuilder.Form);

            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (parameters[key] != null)
                    {
                        requestBodyBuilder.AddFormDataPart(key, parameters[key]);
                    }
                }
            }
            return requestBodyBuilder;
        }
        FileUploadResponse MakeRequest(string url,  MultipartBuilder requestBodyBuilder, IDictionary<string, string> headers = null)
        {
            RequestBody requestBody = requestBodyBuilder.Build();

            var requestBuilder = new Request.Builder();

            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (!string.IsNullOrEmpty(headers[key]))
                    {
                        requestBuilder = requestBuilder.AddHeader(key, headers[key]);
                    }
                }
            }

            Request request = requestBuilder
                .Url(url)
                .Post(requestBody)
                .Build();

            OkHttpClient client = new OkHttpClient();
            client.SetConnectTimeout(5, TimeUnit.Minutes); // connect timeout
            client.SetReadTimeout(5, TimeUnit.Minutes);    // socket timeout

            Response response = client.NewCall(request).Execute();
            var responseString = response.Body().String();
            var code = response.Code();
            var fileUploadResponse = new FileUploadResponse(responseString, code);
            if (response.IsSuccessful)
            {

                FileUploadCompleted(this, fileUploadResponse);

                return fileUploadResponse;
            }
            else
            {

                FileUploadError(this, fileUploadResponse);
                return fileUploadResponse;
            }
        }
    }
}