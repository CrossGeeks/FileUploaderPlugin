using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FileUploader.Abstractions
{
    public class FileUploadResponse
    {
        public string Tag { get; }
        public string Message { get; }
        public int StatusCode { get; }
        public FileUploadResponse(string message, int statuCode,string tag)
        {
            Message = message;
            StatusCode = statuCode;
            Tag = tag;
        }
    }
}
