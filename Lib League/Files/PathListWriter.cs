using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace League.Files
{
    public class PathListWriter
    {
        private MemoryStream _stream;
        private BinaryWriter _writer;
        private List<string> _paths;

        public PathListWriter() { }

        public void Write(string filename, List<string> paths)
        {
            _paths = paths;
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);

            WriteStringList();

            File.WriteAllBytes(filename, _stream.ToArray());
        }

        private void WriteStringList()
        {
            _writer.Write((uint)_paths.Count);
            for (int i = 0; i < _paths.Count; i++)
                WriteString(i);
        }

        private void WriteString(int id)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(_paths[id]);
            _writer.Write((uint)bytes.Length);
            _writer.Write(bytes, 0, bytes.Length);
        }
    }
}
