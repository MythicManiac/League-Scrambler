using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace League.Files
{
    public class PathListReader
    {
        private MemoryStream _stream;
        private BinaryReader _reader;
        private string[] paths;

        public PathListReader() { }

        public string[] Read(string filepath)
        {
            return Read(File.ReadAllBytes(filepath));
        }

        public string[] Read(byte[] data)
        {
            _stream = new MemoryStream(data);
            _stream.Seek(0, SeekOrigin.Begin);
            _reader = new BinaryReader(_stream);

            ReadStringList();

            return paths;
        }

        public void ReadStringList()
        {
            paths = new string[_reader.ReadUInt32()];
            for (int i = 0; i < paths.Length; i++)
                paths[i] = ReadString();
        }

        public string ReadString()
        {
            uint count = _reader.ReadUInt32();
            byte[] bytes = new byte[count];
            _reader.Read(bytes, 0, bytes.Length);
            return ASCIIEncoding.ASCII.GetString(bytes, 0, bytes.Length);
        }
    }
}
