using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueCommon.Files
{
    public class RiotFile
    {
        public FileEntry FileMetadata { get; private set; }
        public FilePath PathMetadata { get; private set; }

        public RiotFile(FileEntry file, FilePath path)
        {
            if (file.Hash != path.Hash)
                throw new Exception("File and path hashesh don't match");

            FileMetadata = file;
            PathMetadata = path;
        }
    }
}
