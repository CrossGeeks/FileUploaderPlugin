using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.FileUploader.Abstractions
{
    public class FileBytesItem
    {
        public string Name { get; }
        public string FieldName { get; }
        public byte[] Bytes { get; }

        public FileBytesItem(string fieldName, byte[] bytes, string name)
        {
            Name = name;
            Bytes = bytes;
            FieldName = fieldName;
        }
    }
}
