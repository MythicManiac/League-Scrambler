using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Hashes
{
    public class SkinHashes
    {
        public readonly uint Blnd;
        public readonly uint Skn;
        public readonly uint Skl;
        public readonly uint Dds;

        public SkinHashes(uint blndFile, uint sknFile, uint sklFile, uint ddsFile)
        {
            Blnd = blndFile;
            Skn = sknFile;
            Skl = sklFile;
            Dds = ddsFile;
        }
    }
}
