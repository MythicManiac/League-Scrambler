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

        private Dictionary<string, ReleaseManifestFileEntry> _pathTable;
        private Dictionary<uint, ArchiveState> _archiveStates;
        private Dictionary<uint, Archive> _archiveTable;

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
            _archiveStates = new Dictionary<uint, ArchiveState>();
            if(File.Exists(LeagueLocations.GetArchiveStatePath(_leaguePath)))
            {
                var states = new ArchiveStateReader().ReadArchiveStates(LeagueLocations.GetArchiveStatePath(_leaguePath));
                for(int i = 0; i < states.Length; i++)
                {
                    _archiveStates[states[i].ArchiveIndex] = states[i];
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
            var archive = _archiveTable[id];

            if (!archive.Files.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the archive");

                Console.WriteLine("Following file was not found: {0}", filepath);
                return null;
            }

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
            if (!_pathTable.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the releasemanifest");

                Console.WriteLine("Following file was not found in the releasemanifest: {0}", filepath);
                return;
            }

            if (!_archiveTable[_pathTable[filepath].ArchiveId].Files.ContainsKey(filepath))
            {
                if (!surpressErrors)
                    throw new FileNotFoundException("File with given path was not found in the archive");

                Console.WriteLine("Following file was not found: {0}", filepath);
                return;
            }

            WriteFile(_pathTable[filepath].ArchiveId, filepath, compress, data, surpressErrors);
        }

        private void WriteFile(uint archiveId, string filepath, bool compress, byte[] data)
        {
            WriteFile(archiveId, filepath, compress, data, false);
        }

        private void WriteFile(uint archiveId, string filepath, bool compress, byte[] data, bool surpressErrors)
        {
            var archive = _archiveTable[archiveId];

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
            if(!_archiveStates.ContainsKey(archiveId))
            {
                var state = new ArchiveState();
                state.ArchiveIndex = archiveId;
                state.OriginalLength = archive.DataLength;
                state.OriginalValues = new Dictionary<string, ArchiveFileInfo>();
                _archiveStates[archiveId] = state;
            }

            var offset = _writer.WriteData(archive, data);

            // Copy file info to the list of originals and then modify it
            if (!_archiveStates[archiveId].OriginalValues.ContainsKey(filepath))
            {
                _archiveStates[archiveId].OriginalValues[filepath] = ArchiveFileInfo.Copy(_archiveTable[archiveId].Files[filepath]);
                archive.Files[filepath].DataLength = (uint)data.Length;
                archive.Files[filepath].DataOffset = (uint)offset;
            }

            // Handle manifest changes
            _pathTable[filepath].Descriptor.CompressedSize = compressedLength;
            _pathTable[filepath].Descriptor.DecompressedSize = uncomporessedLength;
        }

        public void WriteStateInfo()
        {
            // Backup manifest
            if (File.Exists(LeagueLocations.GetManifestStatePath(_leaguePath)))
                File.Delete(LeagueLocations.GetManifestStatePath(_leaguePath));

            File.Copy(LeagueLocations.GetManifestPath(_leaguePath), LeagueLocations.GetManifestStatePath(_leaguePath));

            foreach(ArchiveState state in _archiveStates.Values)
            {
                _writer.WriteArchive(_archiveTable[state.ArchiveIndex], _archiveTable[state.ArchiveIndex].FilePath);
                Console.WriteLine("Archive written to {0}", _archiveTable[state.ArchiveIndex].FilePath);
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
                var archive = _archiveTable[states[i].ArchiveIndex];
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
            _archiveStates = new Dictionary<uint, ArchiveState>();
            _manifest = ReleaseManifest.LoadFromFile(LeagueLocations.GetManifestPath(_leaguePath));
            LoadManifestPaths();
            WriteStateInfo();

            // Remove the corrupt data flag if it exists
            if (File.Exists(LeagueLocations.GetCorruptFlagPath(_leaguePath)))
                File.Delete(LeagueLocations.GetCorruptFlagPath(_leaguePath));
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
