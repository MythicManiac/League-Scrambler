using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zlib;
using League.Files;
using League.Files.Manifest;
using League.Utils;

namespace League.Tools
{
    public class ArchiveFileManager
    {
        private ReleaseManifest _manifest;

        private Dictionary<string, ArchiveState> _archiveStates;
        private Dictionary<string, ReleaseManifestFileEntry> _pathTable;
        private Dictionary<uint, Archive> _archiveTable;

        private ArchiveReader _reader;
        private ArchiveWriter _writer;

        private string _leaguePath;

        public ArchiveFileManager(string leaguePath)
        {
            _leaguePath = leaguePath;
            _reader = new ArchiveReader();
            _writer = new ArchiveWriter();

            LoadManifestPaths();
            LoadArchiveStates();
            LoadArchives();
        }

        private void LoadManifestPaths()
        {
            _manifest = ReleaseManifest.LoadFromFile(LeagueLocations.GetManifestPath(_leaguePath));
            _pathTable = new Dictionary<string, ReleaseManifestFileEntry>();

            for(int i = 0; i < _manifest.Files.Length; i++)
            {
                if(_manifest.Files[i].EntityType != 4)
                    _pathTable[_manifest.Files[i].FullName] = _manifest.Files[i];
            }
        }

        private void LoadArchiveStates()
        {
            _archiveStates = new Dictionary<string, ArchiveState>();
            if(File.Exists(LeagueLocations.GetArchiveStatePath(_leaguePath)))
            {
                var states = new ArchiveStateReader().ReadArchiveStates(LeagueLocations.GetArchiveStatePath(_leaguePath));
                for(int i = 0; i < states.Length; i++)
                {
                    _archiveStates[states[i].ArchivePath] = states[i];
                }
            }
        }

        private void LoadArchives()
        {
            _archiveTable = new Dictionary<uint, Archive>();
            var files = Directory.EnumerateFiles(LeagueLocations.GetArchivePath(_leaguePath), "*.raf", SearchOption.AllDirectories).ToArray();
            for(int i = 0; i < files.Length; i++)
            {
                var archive = _reader.ReadArchive(files[i]);
                _archiveTable[archive.GetManagerIndex()] = archive;
            }
        }

        public byte[] ReadFile(string filepath, bool uncompress)
        {
            return ReadFile(filepath, uncompress, false);
        }

        public byte[] ReadFile(string filepath, bool uncompress, bool surpressErrors)
        {
            if(!_pathTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the releasemanifest");

                Console.WriteLine("Following file was not found in the releasemanifest: {0}", filepath);
                return null;
            }

            var id = _pathTable[filepath].ArchiveId;
            if(id == 0)
                return null;

            var archive = _archiveTable[id];
            var info = archive.Files[filepath];
            var file = _reader.ReadData(archive, info.DataOffset, info.DataLength);

            if (uncompress)
                file = ZlibStream.UncompressBuffer(file);

            return file;
        }

        public void WriteFile(string filepath, bool compress, byte[] data)
        {
            WriteFile(filepath, compress, data, false);
        }

        public void WriteFile(string filepath, bool compress, byte[] data, bool surpressErrors)
        {

        }

        public void Revert()
        {

        }

        public string[] GetAllFilePaths()
        {
            return _pathTable.Keys.ToArray();
        }

        public ReleaseManifestFileEntry[] GetAllFileEntries()
        {
            return _manifest.Files;
        }
    }
}
