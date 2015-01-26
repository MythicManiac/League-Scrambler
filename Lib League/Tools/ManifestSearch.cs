using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using League.Files.Manifest;
using League.Files;

namespace League.Tools
{
    public class ManifestSearch
    {
        private ReleaseManifest _manifest;
        private ArchiveFileManager _manager;

        public ManifestSearch(ArchiveFileManager manager)
        {
            _manager = manager;
            _manifest = manager.Manifest;
        }

        public string[] FindLoadingScreens()
        {
            var result = new List<string>();
            var files = _manifest.Root.GetChildDirectoryOrNull("DATA").GetChildDirectoryOrNull("Characters").GetAllSubfiles().ToArray();
            for(int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.ToLower().Contains("loadscreen") && files[i].Name.Split('.').Last() == "dds")
                    result.Add(files[i].FullName);
            }
            return result.ToArray();
        }

        public Character[] FindCharacters(string[] types)
        {
            var skins = new List<Character>();
            var directories = _manifest.Root.GetChildDirectoryOrNull("DATA").GetChildDirectoryOrNull("Characters").Directories.ToArray();

            for (int i = 0; i < directories.Length; i++)
            {
                var character = directories[i].GetChildFileOrNull(directories[i].Name + ".inibin");

                if (character == null)
                {
                    Console.WriteLine("Couldn't find file {0}.inibin from the releasemanifest", directories[i].FullName);
                    continue;
                }

                var inibin = new Character(new Inibin(_manager.ReadFile(character.FullName).Uncompress(), character.FullName));

                var flag = false;
                for (int j = 0; j < types.Length; j++)
                {
                    if (inibin.Type.ToLower().Contains(types[j].ToLower()))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    skins.Add(inibin);

                    var subdir = directories[i].GetChildDirectoryOrNull("Skins");
                    if (subdir != null)
                    {
                        var skindirs = subdir.Directories.ToArray();
                        for (int j = 0; j < skindirs.Length; j++)
                        {
                            var skin = skindirs[j].GetChildFileOrNull(skindirs[j].Name + ".inibin");

                            if (skin == null)
                            {
                                Console.WriteLine("Couldn't find file {0}.inibin from the releasemanifest", skindirs[j].FullName);
                                continue;
                            }

                            skins.Add(new Character(new Inibin(_manager.ReadFile(skin.FullName).Uncompress(), skin.FullName)));
                        }
                    }
                }
            }

            return skins.ToArray();
        }

        public string FindClosestPath(string filename, string start, string stop)
        {
            var file = FindClosest(filename, start, stop);

            if (file == null)
            {
                //Console.WriteLine("[SPECIAL SEARCH] - {0}", start);
                if (start.Contains("Udyr"))
                {
                    file = FindClosest(filename, "DATA/Characters/Udyr", stop);
                }
                else if (start.Contains("Nasus"))
                {
                    file = FindClosest(filename, "DATA/Characters/Nasus", stop);
                }
                else if (start.Contains("VisionWard"))
                {
                    file = FindClosest(filename, "DATA/Characters/SightWard", stop);
                }
                else if (start.Contains("Thresh"))
                {
                    file = FindClosest(filename, "DATA/Characters/Thresh", stop);
                }
                else if (start.Contains("Maokai"))
                {
                    file = FindClosest(filename, "DATA/Characters/Maokai", stop);
                }
                else if (start.Contains("KogMaw"))
                {
                    file = FindClosest(filename, "DATA/Characters/KogMaw", stop);
                }
                else if (start.Contains("ZyraPassive"))
                {
                    file = FindClosest(filename, "DATA/Characters/ZyraThornPlant", stop);
                }
                else if (start.Contains("Swain"))
                {
                    file = FindClosest(filename, "DATA/Characters/Swain", stop);
                }
                else if (start.Contains("Orianna"))
                {
                    file = FindClosest(filename, "DATA/Characters/Orianna", stop);
                }
                else if (start.Contains("MonkeyKing"))
                {
                    file = FindClosest(filename, "DATA/Characters/MonkeyKing", stop);
                }
                else if (start.Contains("Odin_Red_Minion_Caster"))
                {
                    file = FindClosest(filename, "DATA/Characters/Red_Minion_Wizard", stop);
                }
                else if (start.Contains("Odin_Blue_Minion_Caster"))
                {
                    file = FindClosest(filename, "DATA/Characters/Blue_Minion_Wizard", stop);
                }
            }

            if (file == null)
            {
                Console.WriteLine("Couldn't find file {0} - {1}", start, filename);
                return null;
            }

            return file.FullName;
        }

        public ReleaseManifestFileEntry FindClosest(string filename, string start, string stop)
        {
            if (start.Last() == '/')
                start = start.Remove(start.Length - 1, 1);

            var dirnames = start.Split('/');
            var directory = _manifest.Root.GetChildDirectoryOrNull(dirnames[0]);
            for (int i = 1; i < dirnames.Length; i++)
            {
                directory = directory.GetChildDirectoryOrNull(dirnames[i]);
            }

            if (directory == null)
            {
                Console.WriteLine("Couldn't find directory for {0}", start);
                return null;
            }

            var subfiles = directory.GetAllSubfiles().ToArray();
            for (int i = 0; i < subfiles.Length; i++)
            {
                if (subfiles[i].Name.Equals(filename, StringComparison.OrdinalIgnoreCase))
                    return subfiles[i];
            }

            directory = directory.Parent;
            while (directory.Name != stop)
            {
                subfiles = directory.GetAllSubfiles().ToArray();
                for (int i = 0; i < subfiles.Length; i++)
                {
                    if (subfiles[i].Name.Equals(filename, StringComparison.OrdinalIgnoreCase))
                        return subfiles[i];
                }
                directory = directory.Parent;
            }

            return null;
        }
    }
}
