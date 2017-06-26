using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FileUploader.Abstractions
{
    public class FilePathItem
    {
        
        public string Path { get; } 
        public string FieldName { get; }

        public FilePathItem(string fieldName,string path)
        {
            Path = path;
            FieldName = fieldName;
        }
    }
}
