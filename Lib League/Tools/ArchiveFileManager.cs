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

        private Dictionary<string, ReleaseManifestFileEntry> _indexTable;
        private Dictionary<string, ArchiveState> _archiveStates;
        private Dictionary<string, Archive> _archiveTable;
        private Dictionary<string, Archive> _fileTable;

        private ArchiveReader _reader;
        private ArchiveWriter _writer;

        private string _leaguePath;

        public bool ArchivesModified
        {
            get
            {
                return _archiveStates.Count > 0;
            }
        }

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
            Console.WriteLine("Loading file info...");
            _manifest = ReleaseManifest.LoadFromFile(LeagueLocations.GetManifestPath(_leaguePath));
            _indexTable = new Dictionary<string, ReleaseManifestFileEntry>();

            for(int i = 0; i < _manifest.Files.Length; i++)
            {
                if(_manifest.Files[i].EntityType != 4)
                    _indexTable[_manifest.Files[i].FullName] = _manifest.Files[i];
            }
        }

        private void LoadArchiveStates()
        {
            Console.WriteLine("Loading state info...");
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
            Console.WriteLine("Indexing files...");
            _archiveTable = new Dictionary<string, Archive>();
            _fileTable = new Dictionary<string, Archive>();
            var files = Directory.EnumerateFiles(LeagueLocations.GetArchivePath(_leaguePath), "*.raf", SearchOption.AllDirectories).ToArray();
            for(int i = 0; i < files.Length; i++)
            {
                var archive = _reader.ReadArchive(files[i]);
                _archiveTable[archive.FilePath] = archive;
                foreach(var kvp in archive.Files)
                {
                    if (!_fileTable.ContainsKey(kvp.Key))
                    {
                        _fileTable[kvp.Key] = archive;
                    }
                    else if (_indexTable.ContainsKey(kvp.Key) && _indexTable[kvp.Key].ArchiveId == archive.GetManagerIndex())
                    {
                        _fileTable[kvp.Key] = archive;
                    }
                }
            }
        }

        public byte[] ReadFile(string filepath, bool uncompress)
        {
            return ReadFile(filepath, uncompress, false);
        }

        public byte[] ReadFile(string filepath, bool uncompress, bool surpressErrors)
        {
            if (!_indexTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the releasemanifest");

                Console.WriteLine("Following file was not found in the releasemanifest: {0}", filepath);
                return null;
            }
            if (!_fileTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the file table");

                Console.WriteLine("Following file was not found: {0}", filepath);
                return null;
            }

            var archive = _fileTable[filepath];
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
            if (!_indexTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the releasemanifest");

                Console.WriteLine("Following file was not found in the releasemanifest: {0}", filepath);
                return;
            }

            if (!_fileTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the file table");

                Console.WriteLine("Following file was not found: {0}", filepath);
                return;
            }

            var archive = _fileTable[filepath];

            // Handle compression and uncompression
            var uncomporessedLength = 0U;
            var compressedLength = 0U;

            if ((data[0] == 0x78 && (data[1] == 0x01 || data[1] == 0x9C || data[1] == 0xDA)))
            {
                compressedLength = (uint)data.Length;
                var temp = ZlibStream.UncompressBuffer(data);
                uncomporessedLength = (uint)temp.Length;
                if (!compress)
                    data = temp;
            }
            else
            {
                uncomporessedLength = (uint)data.Length;
                var temp = ZlibStream.CompressBuffer(data);
                compressedLength = (uint)temp.Length;
                if (compress)
                    data = temp;
            }

            // Handle archive state creation
            if(!_archiveStates.ContainsKey(archive.FilePath))
            {
                var state = new ArchiveState();
                state.ArchivePath = archive.FilePath;
                state.OriginalLength = archive.DataLength;
                state.OriginalValues = new Dictionary<string, ArchiveFileInfo>();
                _archiveStates[archive.FilePath] = state;
            }

            var offset = _writer.WriteData(archive, data);

            // Copy file info to the list of originals and then modify it
            if (!_archiveStates[archive.FilePath].OriginalValues.ContainsKey(filepath))
            {
                _archiveStates[archive.FilePath].OriginalValues[filepath] = ArchiveFileInfo.Copy(_fileTable[filepath].Files[filepath]);
                archive.Files[filepath].DataLength = (uint)data.Length;
                archive.Files[filepath].DataOffset = (uint)offset;
            }

            // Handle manifest changes
            _indexTable[filepath].Descriptor.CompressedSize = compressedLength;
            _indexTable[filepath].Descriptor.DecompressedSize = uncomporessedLength;
        }

        public void WriteStateInfo()
        {
            // Backup manifest
            if (File.Exists(LeagueLocations.GetManifestStatePath(_leaguePath)))
                File.Delete(LeagueLocations.GetManifestStatePath(_leaguePath));

            File.Copy(LeagueLocations.GetManifestPath(_leaguePath), LeagueLocations.GetManifestStatePath(_leaguePath));

            foreach(ArchiveState state in _archiveStates.Values)
            {
                _writer.WriteArchive(_archiveTable[state.ArchivePath], _archiveTable[state.ArchivePath].FilePath);
                Console.WriteLine("Archive written to {0}", state.ArchivePath);
            }
            var writer = new ArchiveStateWriter();
            writer.WriteArchiveStates(_archiveStates.Values.ToArray(), LeagueLocations.GetArchiveStatePath(_leaguePath));
            _manifest.SaveChanges();
        }

        public void Revert()
        {
            if (_archiveStates.Count == 0)
                return;

            var states = _archiveStates.Values.ToArray();


            // Reverse archives
            for(int i = 0; i < states.Length; i++)
            {
                var archive = _archiveTable[states[i].ArchivePath];
                foreach(var entry in states[i].OriginalValues)
                {
                    archive.Files[entry.Key] = entry.Value;
                }
                _writer.WriteArchive(archive, archive.FilePath);
                _writer.SetDataLength(archive, states[i].OriginalLength);
                archive.DataLength = states[i].OriginalLength;
            }

            // Reverse manifest
            File.WriteAllBytes(LeagueLocations.GetManifestPath(_leaguePath), File.ReadAllBytes(LeagueLocations.GetManifestStatePath(_leaguePath)));
            File.Delete(LeagueLocations.GetManifestStatePath(_leaguePath));

            // Clear local variables and save them
            _archiveStates = new Dictionary<string, ArchiveState>();
            _manifest = ReleaseManifest.LoadFromFile(LeagueLocations.GetManifestPath(_leaguePath));
            LoadManifestPaths();
            WriteStateInfo();

            // Remove the corrupt data flag if it exists
            if (File.Exists(LeagueLocations.GetCorruptFlagPath(_leaguePath)))
                File.Delete(LeagueLocations.GetCorruptFlagPath(_leaguePath));
        }

        public string[] GetAllFileIndexes()
        {
            return _indexTable.Keys.ToArray();
        }

        public ReleaseManifestFileEntry[] GetAllFileEntries()
        {
            return _manifest.Files;
        }
    }
}
