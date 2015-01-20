using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueCommon
{
    public class Skin : IEquatable<Skin>
    {
        public string DdsFile { get; set; }
        public string SklFile { get; set; }
        public string SknFile { get; set; }
        public string BlndFile { get; set; }

        public int Id { get; set; }

        public Skin() { }

        public Skin(string dds, string skl, string skn, string blnd)
        {
            DdsFile = dds;
            SklFile = skl;
            SknFile = skn;
            BlndFile = blnd;
        }

        public bool Equals(Skin other)
        {
            return DdsFile == other.DdsFile && SklFile == other.SklFile && SknFile == other.SknFile && BlndFile == other.BlndFile;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Skin);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
