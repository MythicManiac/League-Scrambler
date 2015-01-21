using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League.Utils;

namespace League.Files.Manifest
{
    public class ReleaseManifest
    {
        public string FileLocation { get; set; }
        public ReleaseManifestHeader Header { get; set; }
        public StringTable Strings { get; set; }
        public ReleaseManifestDirectoryEntry Root { get; set; }
        public ReleaseManifestFileEntry[] Files { get; set; }
        public ReleaseManifestDirectoryEntry[] Directories { get; set; }

        public ReleaseManifest() { }

        public void Dump()
        {
            Table table = new Table(3);
            for(int i = 0; i < Files.Length; i++)
            {
                table.AddRow(Files[i].FullName, Files[i].Name, Files[i].ArchiveId);
            }
            table.Dump(8);
        }

        public List<string> GetFilePaths(string filename, string foldername)
        {
            List<string> result = new List<string>();
            for(int i = 0; i < Files.Length; i++)
            {
                if (Files[i].FullName.Contains(foldername) && Path.GetFileName(Files[i].FullName.ToLower()) == filename.ToLower())
                    result.Add(Files[i].FullName);
            }
            return result;
        }

        public void SaveChanges()
        {
            ReleaseManifestWriter writer = new ReleaseManifestWriter(this);
            writer.Save(FileLocation);
        }

        public static ReleaseManifest LoadFromFile(string path)
        {
            ReleaseManifestReader reader = new ReleaseManifestReader(path);
            return reader.Read();
        }
    }
}
