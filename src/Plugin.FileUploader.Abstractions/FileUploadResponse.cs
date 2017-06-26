using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FileUploader.Abstractions
{
    public class FileUploadResponse
    {
        public string Message { get; }
        public int StatusCode { get; }
        public FileUploadResponse(string message, int statuCode)
        {
            Message = message;
            StatusCode = statuCode;
        }
    }
}
