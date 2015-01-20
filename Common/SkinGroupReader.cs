using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueCommon
{
    public class SkinGroupReader
    {
        private MemoryStream _stream;
        private BinaryReader _reader;
        private SkinGroup[] _result;

        public SkinGroupReader() { }

        public SkinGroup[] Read(string path)
        {
            return Read(new MemoryStream(File.ReadAllBytes(path)));
        }

        public SkinGroup[] Read(byte[] file)
        {
            return Read(new MemoryStream(file));
        }

        public SkinGroup[] Read(MemoryStream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(_stream);
            _stream.Seek(0, SeekOrigin.Begin);

            ReadSkinGroups();

            return _result;
        }

        private void ReadSkinGroups()
        {
            _result = new SkinGroup[_reader.ReadUInt32()];
            for(uint i = 0; i < _result.Length; i++)
            {
                _result[i] = ReadSkinGroup();
            }
        }

        private SkinGroup ReadSkinGroup()
        {
            SkinGroup result = new SkinGroup();
            uint count = _reader.ReadUInt32();
            for(uint i = 0; i < count; i++)
            {
                result.Skins.Add(ReadSkin());
            }
            return result;
        }

        private Skin ReadSkin()
        {
            Skin result = new Skin();
            result.BlndFile = ReadString();
            result.DdsFile = ReadString();
            result.SklFile = ReadString();
            result.SknFile = ReadString();
            return result;
        }

        private string ReadString()
        {
            uint length = _reader.ReadUInt32();
            byte[] buffer = new byte[length];
            _reader.Read(buffer, 0, buffer.Length);
            return ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length);
        }
    }
}
