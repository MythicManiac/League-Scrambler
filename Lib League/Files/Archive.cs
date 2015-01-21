using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Files
{
    public class Archive
    {
        public static readonly uint MAGIC_VALUE = 0x18be0ef0;

        public string FilePath { get { return _filePath; } internal set { _filePath = value; DataFilePath = value + ".dat"; } }
        public string DataFilePath { get; private set; }
        public uint ArchiveIndex { get; internal set; }
        public uint ArchiveVersion { get; internal set; }
        public Dictionary<string, ArchiveFileInfo> FileList { get; internal set; }

        private string _filePath;
    }

    public class ArchiveFileInfo
    {
        public string Path { get; set; }
        public long DataOffset { get; set; }
        public long DataLength { get; set; }
    }
}
