using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Package
{
    public interface IEaglePackageReaderExtractionHandler
    {
        void BeginCopyFile(string src);
        void FinishCopyFile(string src);
        void CopyFileConflict(string src);
    }
}
