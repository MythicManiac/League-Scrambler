using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League.Files;
using League.Files.Manifest;
using League.Tools;
using League.Hashes;

namespace League
{
    public class Character
    {
        public Inibin Inibin { get; private set; }
        public Skin[] Skins { get; set; }

        public Character(byte[] data, string path) : this(Inibin.DeserializeInibin(data, path)) { }

        public Character(Inibin inibin)
        {
            Inibin = inibin;
        }

        public void LoadSkins(ManifestSearch search)
        {
            List<Skin> skinList = new List<Skin>();
            for(int i = 0; i < 9; i++)
            {
                Skin current = GetSkin(i, search);
                if (current != null)
                    skinList.Add(current);
            }
            Skins = skinList.ToArray();
        }

        public string Type
        {
            get
            {
                if (Inibin.Content.ContainsKey(CharacterHashes.Type))
                    return (string)Inibin.Content[CharacterHashes.Type].Value;

                return "Unknown";
            }
        }

        public string NameKey
        {
            get
            {
                if (Inibin.Content.ContainsKey(CharacterHashes.NameKey))
                    return (string)Inibin.Content[CharacterHashes.NameKey].Value;
                
                return "Unknown";
            }
        }

        public string SquareIcon
        {
            get
            {
                if (Inibin.Content.ContainsKey(CharacterHashes.SquareIconDds))
                    return (string)Inibin.Content[CharacterHashes.SquareIconDds].Value;

                return null;
            }
        }

        public string CircleIcon
        {
            get
            {
                if (Inibin.Content.ContainsKey(CharacterHashes.CircleIconDds))
                    return (string)Inibin.Content[CharacterHashes.CircleIconDds].Value;

                return null;
            }
        }

        private int SkinCount
        {
            get
            {
                for (int i = 0; i < CharacterHashes.Skins.Length; i++)
                {
                    if (!HasSkin(i))
                        return i;
                }

                return CharacterHashes.Skins.Length;
            }
        }

        public bool HasSkin(int id)
        {
            SkinHashes hash = CharacterHashes.Skins[id];
            return Inibin.Content.ContainsKey(hash.Blnd) && Inibin.Content.ContainsKey(hash.Skn) && Inibin.Content.ContainsKey(hash.Skl) && Inibin.Content.ContainsKey(hash.Dds);
        }

        private Skin GetSkin(int id, ManifestSearch search)
        {
            if (HasSkin(id))
            {
                Skin result = new Skin();
                SkinHashes hash = CharacterHashes.Skins[id];

                var start = Inibin.FilePath.Substring(0, Inibin.FilePath.LastIndexOf('/'));

                result.SknFile = search.FindClosestPath((string)Inibin.Content[hash.Skn].Value, start, "Characters");
                result.SklFile = search.FindClosestPath((string)Inibin.Content[hash.Skl].Value, start, "Characters");
                result.DdsFile = search.FindClosestPath((string)Inibin.Content[hash.Dds].Value, start, "Characters");
                result.BlndFile = search.FindClosestPath((string)Inibin.Content[hash.Blnd].Value, start, "Characters");

                if (result.BlndFile == null || result.SknFile == null || result.DdsFile == null || result.SklFile == null)
                    return null;

                result.Id = id;

                return result;
            }

            return null;
        }
    }
}
