using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League.Files;
using League.Files.Manifest;
using League.Hashes;

namespace League
{
    public class Character
    {
        public Inibin Inibin { get; private set; }

        public Character(string inibin, string root = "") : this(new Inibin(inibin, root)) { }
        public Skin[] Skins { get; set; }

        public Character(Inibin inibin)
        {
            Inibin = inibin;
            Inibin.Read();
        }

        public void LoadSkins(ReleaseManifest manifest)
        {
            List<Skin> skinList = new List<Skin>();
            for(int i = 0; i < 9; i++)
            {
                Skin current = GetSkin(i, manifest);
                if (current != null)
                    skinList.Add(current);
            }
            Skins = skinList.ToArray();
            if(Skins.Length == 0 && (Type.ToLower().Contains("minion") || Type.ToLower().Contains("monster")))
            {
                Console.WriteLine("Found {0} skins for {1}", Skins.Length, Inibin.InibinPath);
            }
        }

        public string Type
        {
            get
            {
                if (Inibin.Data.ContainsKey(CharacterHashes.Type))
                    return (string)Inibin.Data[CharacterHashes.Type];

                return "Unknown";
            }
        }

        public string NameKey
        {
            get
            {
                if (Inibin.Data.ContainsKey(CharacterHashes.NameKey))
                    return (string)Inibin.Data[CharacterHashes.NameKey];
                
                return "Unknown";
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
            return Inibin.Data.ContainsKey(hash.Blnd) && Inibin.Data.ContainsKey(hash.Skn) && Inibin.Data.ContainsKey(hash.Skl) && Inibin.Data.ContainsKey(hash.Dds);
        }

        private Skin GetSkin(int id, ReleaseManifest manifest)
        {
            if (HasSkin(id))
            {
                Skin result = new Skin();
                SkinHashes hash = CharacterHashes.Skins[id];

                result.SknFile = SearchFile(Inibin.Data[hash.Skn], manifest);
                result.SklFile = SearchFile(Inibin.Data[hash.Skl], manifest);
                result.DdsFile = SearchFile(Inibin.Data[hash.Dds], manifest);
                result.BlndFile = SearchFile(Inibin.Data[hash.Blnd], manifest);

                if (result.BlndFile == null || result.SknFile == null || result.DdsFile == null || result.SklFile == null)
                    return null;

                result.Id = id;

                return result;
            }

            return null;
        }

        private string SearchFile(object input, ReleaseManifest manifest)
        {
            string filename = (string)input;
            List<string> matches = manifest.GetFilePaths(filename, "DATA/Characters/");
            if (matches.Count > 1)
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if(matches[i].Contains(Path.GetDirectoryName(Inibin.LeaguePath).Replace('\\', '/') + '/'))
                    {
                        //Console.WriteLine("[MATCH] - {0} - {1} - {2}", Inibin.LeaguePath, filename, matches[i]);
                        return matches[i];
                    }
                }
                if(Path.GetFileNameWithoutExtension(Inibin.LeaguePath) == "MonkeyKingClone")
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        if (matches[i].Contains("DATA/Characters/MonkeyKing/"))
                        {
                            //Console.WriteLine("[MATCH SPECIAL] - {0} - {1} - {2}", Inibin.LeaguePath, filename, matches[i]);
                            return matches[i];
                        }
                    }
                }
                if (Path.GetFileNameWithoutExtension(Inibin.LeaguePath) == "OriannaNoBall")
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        if (matches[i].Contains("DATA/Characters/Orianna/"))
                        {
                            //Console.WriteLine("[MATCH SPECIAL] - {0} - {1} - {2}", Inibin.LeaguePath, filename, matches[i]);
                            return matches[i];
                        }
                    }
                }
                if (Inibin.LeaguePath.ToLower().Contains("skins"))
                {
                    for(int i = 0; i < matches.Count; i++)
                    {
                        if(matches[i].ToLower().Contains("/skins/base/"))
                        {
                            //Console.WriteLine("[MATCH NEW FORMAT] - {0} - {1} - {2}", Inibin.LeaguePath, filename, matches[i]);
                            return matches[i];
                        }
                    }
                }
                for (int i = 0; i < matches.Count; i++)
                {
                    Console.WriteLine("[AMBIGIOUS] - {0} - {1} - {2}", Inibin.LeaguePath, filename, matches[i]);
                }
                return null;
            }
            if(matches.Count > 0)
                return matches[0];
            else
            {
                Console.WriteLine("No matches found for {0}", filename);
                return null;
            }
        }
    }
}
