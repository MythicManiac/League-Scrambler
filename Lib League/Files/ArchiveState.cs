using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Files
{
    public class ArchiveState
    {
        public uint ArchiveIndex { get; set; }
        public long OriginalLength { get; set; }
        public Dictionary<string, ArchiveFileInfo> OriginalValues { get; set; }
    }
}
