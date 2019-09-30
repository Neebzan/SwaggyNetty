using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    public class FileTransferModel
    {
        public List<FileModel> Files { get; set; } = new List<FileModel>();
        public long TotalSize { get; set; }
    }

    public class FileModel
    {
        public string FilePath { get; set; }
        public long Size { get; set; }
    }
}
