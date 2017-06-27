using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Runtime.InteropServices;

namespace Plugin.FileUploader
{
    internal class NativeTools
    {
        [DllImport(ObjCRuntime.Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCopyPreferredTagWithClass")]
        public extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);

        [DllImport(ObjCRuntime.Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCreatePreferredIdentifierForTag")]
        public extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
    }

}