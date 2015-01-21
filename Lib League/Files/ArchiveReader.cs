using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League.Hashes;

namespace League.Files
{
    public class ArchiveReader
    {
        MemoryStream _stream;
        BinaryReader _reader;

        uint _fileListOffset;
        uint _pathListOffset;

        ArchiveFileListEntry[] _fileList;
        ArchivePathListEntry[] _pathList;

        Archive _archive;

        public ArchiveReader() { }

        public Archive ReadArchive(string filepath)
        {
            return ReadArchive(File.ReadAllBytes(filepath), filepath);
        }

        public Archive ReadArchive(byte[] data, string filepath)
        {
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream);

            _archive = new Archive();
            _archive.FilePath = filepath;

            DeserializeArchive();

            return _archive;
        }

        private void DeserializeArchive()
        {
            DeserializeHeader();
            DeserializeFileList();
            DeserializePathList();
            ProcessData();
        }

        private void DeserializeHeader()
        {
            // Skip the magic number (first 4 bytes), no need to read it
            _stream.Seek(4, SeekOrigin.Begin);

            _archive.ArchiveVersion = _reader.ReadUInt32();
            _archive.ArchiveIndex = _reader.ReadUInt32();
            _fileListOffset = _reader.ReadUInt32();
            _pathListOffset = _reader.ReadUInt32();
        }

        private void DeserializeFileList()
        {
            _stream.Seek(_fileListOffset, SeekOrigin.Begin);

            _fileList = new ArchiveFileListEntry[_reader.ReadUInt32()];
            for (uint i = 0; i < _fileList.Length; i++)
                _fileList[i] = DeserializeFileListEntry();
        }

        private ArchiveFileListEntry DeserializeFileListEntry()
        {
            var result = new ArchiveFileListEntry();
            result.PathHash = _reader.ReadUInt32();
            result.DataOffset = _reader.ReadUInt32();
            result.DataLength = _reader.ReadUInt32();
            result.PathIndex = _reader.ReadUInt32();
            return result;
        }

        private void DeserializePathList()
        {
            // We don't care about how many bytes the path list has, so skip first 4 bytes.
            _stream.Seek(_pathListOffset + 4, SeekOrigin.Begin);

            _pathList = new ArchivePathListEntry[_reader.ReadUInt32()];
            for (uint i = 0; i < _pathList.Length; i++)
                _pathList[i] = DeserializePathListEntry();
        }

        private ArchivePathListEntry DeserializePathListEntry()
        {
            var result = new ArchivePathListEntry();
            result.PathOffset = _reader.ReadUInt32();
            result.PathLength = _reader.ReadUInt32();
            return result;
        }

        private void ProcessData()
        {
            _archive.FileList = new Dictionary<string, ArchiveFileInfo>();

            for(uint i = 0; i < _fileList.Length; i++)
            {
                var result = new ArchiveFileInfo();
                result.Path = ReadString(_pathList[_fileList[i].PathIndex].PathOffset, _pathList[_fileList[i].PathIndex].PathLength - 1);

                if(_fileList[i].PathHash != HashFunctions.LeagueHash(result.Path))
                {
                    Console.WriteLine("Invalid hash for a file, skipping....");
                    continue;
                }

                result.DataOffset = _fileList[i].DataOffset;
                result.DataLength = _fileList[i].DataLength;

                _archive.FileList.Add(result.Path, result);
            }
        }

        private string ReadString(uint offset, uint length)
        {
            var buffer = new byte[length];
            _stream.Seek(offset, SeekOrigin.Begin);
            _stream.Read(buffer, 0, buffer.Length);
            return ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length);
        }
    }

    public class ArchiveFileListEntry
    {
        public uint PathHash { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLength { get; set; }
        public uint PathIndex { get; set; }
    }

    public class ArchivePathListEntry
    {
        public uint PathOffset { get; set; }
        public uint PathLength { get; set; }
    }
}
