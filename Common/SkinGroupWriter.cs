using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueCommon
{
    public class SkinGroupWriter
    {
        private SkinGroup[] SkinGroups { get; set; }
        private MemoryStream _stream;
        private BinaryWriter _writer;

        public SkinGroupWriter() { }

        public void Write(string filename, List<SkinGroup> skinGroups)
        {
            Write(filename, skinGroups.ToArray());
        }

        public void Write(string filename, SkinGroup[] skinGroups)
        {
            if (string.IsNullOrEmpty(filename))
                throw new Exception("Must specify a filename before writing");

            SkinGroups = skinGroups;

            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);

            WriteSkinGroups();

            File.WriteAllBytes(filename, _stream.ToArray());
        }

        private void WriteSkinGroups()
        {
            _writer.Write((uint)SkinGroups.Length);
            for(int i = 0; i < SkinGroups.Length; i++)
            {
                WriteSkinGroup(SkinGroups[i]);
            }
        }

        private void WriteSkinGroup(SkinGroup group)
        {
            _writer.Write((uint)group.Skins.Count);
            for(int i = 0; i < group.Skins.Count; i++)
            {
                WriteSkin(group.Skins[i]);
            }
        }

        private void WriteSkin(Skin skin)
        {
            WriteString(skin.BlndFile);
            WriteString(skin.DdsFile);
            WriteString(skin.SklFile);
            WriteString(skin.SknFile);
        }

        private void WriteString(string value)
        {
            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(value);
            _writer.Write((uint)buffer.Length);
            _writer.Write(buffer, 0, buffer.Length);
        }
    }
}
