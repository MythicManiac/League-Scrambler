using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueCommon
{
    public class SkinGroup : IEquatable<SkinGroup>
    {
        public List<Skin> Skins { get; private set; }
        public SkinGroup()
        {
            Skins = new List<Skin>();
        }

        public SkinGroup(Skin initial)
        {
            Skins = new List<Skin>();
            Skins.Add(initial);
        }

        public bool Common(Skin other)
        {
            for (int i = 0; i < Skins.Count; i++)
            {
                if (Skins[i].BlndFile == other.BlndFile || 
                    Skins[i].DdsFile == other.DdsFile || 
                    Skins[i].SknFile == other.SknFile || 
                    Skins[i].SklFile == other.SklFile)
                    return true;
            }
            return false;
        }

        public bool Common(SkinGroup other)
        {
            for (int i = 0; i < Skins.Count; i++)
            {
                for (int j = 0; j < other.Skins.Count; j++)
                {
                    if (Skins[i].BlndFile == other.Skins[j].BlndFile ||
                        Skins[i].DdsFile == other.Skins[j].DdsFile || 
                        Skins[i].SknFile == other.Skins[j].SknFile || 
                        Skins[i].SklFile == other.Skins[j].SklFile)
                        return true;
                }
            }
            return false;
        }

        public bool Equals(Skin other)
        {
            for (int i = 0; i < Skins.Count; i++)
            {
                if (Skins[i].BlndFile == other.BlndFile &&
                    Skins[i].DdsFile == other.DdsFile &&
                    Skins[i].SknFile == other.SknFile &&
                    Skins[i].SklFile == other.SklFile)
                    return true;
            }
            return false;
        }

        public bool Equals(SkinGroup other)
        {
            for (int i = 0; i < Skins.Count; i++)
            {
                for (int j = 0; j < other.Skins.Count; j++)
                {
                    if (Skins[i].BlndFile == other.Skins[j].BlndFile &&
                        Skins[i].DdsFile == other.Skins[j].DdsFile &&
                        Skins[i].SknFile == other.Skins[j].SknFile &&
                        Skins[i].SklFile == other.Skins[j].SklFile)
                        return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(Skin))
                return Equals(obj as Skin);
            if (obj.GetType() == typeof(SkinGroup))
                return Equals(obj as SkinGroup);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
