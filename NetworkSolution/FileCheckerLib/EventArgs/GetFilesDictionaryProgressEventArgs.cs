using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckerLib
{
    public class GetFilesDictionaryProgressEventArgs : EventArgs
    {
        public int FilesFound { get; set; }
        public int ChecksumsGenerated { get; set; }
    }
}
