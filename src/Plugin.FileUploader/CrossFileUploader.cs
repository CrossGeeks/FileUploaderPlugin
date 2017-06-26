using Plugin.FileUploader.Abstractions;
using System;

namespace Plugin.FileUploader
{
  /// <summary>
  /// Cross platform FileUploader implemenations
  /// </summary>
  public class CrossFileUploader
  {
    static Lazy<IFileUploader> Implementation = new Lazy<IFileUploader>(() => CreateFileUploader(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IFileUploader Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IFileUploader CreateFileUploader()
    {
#if PORTABLE
        return null;
#else
        return new FileUploadManager();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
