using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League;
using League.Utils;
using League.Files.Manifest;
using League.Files;

namespace Skin_Finder
{
    class Program
    {
        static ReleaseManifest Manifest { get; set; }
        static List<Character> characterFiles;
        static List<SkinGroup> skinGroups;

        static void Main(string[] args)
        {
            Console.SetOut(new Log("Output.txt"));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            characterFiles = new List<Character>();
            skinGroups = new List<SkinGroup>();
            //SearchFolder(@"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\", ".inibin", 1);

            Manifest = ReleaseManifest.LoadFromFile(@"C:\Games\League of Legends\RADS\projects\lol_game_client\releases\0.0.1.11\releasemanifest");

            string iconPath = @"C:\Users\Mythic\Desktop\LolFiles\DATA\Spells\Icons2D\";

            string[] icons = Directory.GetFiles(iconPath);
            List<string> iconPaths = new List<string>();
            for (int i = 0; i < icons.Length; i++)
            {
                iconPaths.Add(icons[i].Remove(0, @"C:\Users\Mythic\Desktop\LolFiles\".Length).Replace('\\', '/'));
            }
            PathListWriter writer = new PathListWriter();
            writer.Write(@"C:\IconPaths.dat", iconPaths);

            Console.WriteLine(iconPaths[0]);

            /*
            for (int i = 0; i < characterFiles.Count; i++)
            {
                if (characterFiles[i].Type.Contains("Minion") ||
                    characterFiles[i].Type.Contains("Monster") ||
                    characterFiles[i].Type.Contains("Champion") ||
                    characterFiles[i].Inibin.InibinPath.ToLower().Contains("skins"))
                {
                    characterFiles[i].LoadSkins(Manifest);
                    for (int j = 0; j < characterFiles[i].Skins.Length; j++)
                    {
                        bool flag = false;
                        for (int k = 0; k < skinGroups.Count; k++)
                        {
                            if (skinGroups[k].Common(characterFiles[i].Skins[j]))
                            {
                                if (!skinGroups[k].Equals(characterFiles[i].Skins[j]))
                                    skinGroups[k].Skins.Add(characterFiles[i].Skins[j]);

                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                            skinGroups.Add(new SkinGroup(characterFiles[i].Skins[j]));
                    }
                }
            }

            SkinGroupWriter writer = new SkinGroupWriter();
            writer.Write(@"C:\SkinGroups.dat", skinGroups);

            
            Table table = new Table(4);
            foreach(SkinGroup group in skinGroups)
            {
                table.AddColumnlessRow(new string('-', 200));
                for (int i = 0; i < group.Skins.Count; i++)
                {
                    table.AddRow(group.Skins[i].BlndFile, group.Skins[i].DdsFile, group.Skins[i].SklFile, group.Skins[i].SknFile);
                }
            }
            table.DumpTable();
            */
            

            stopwatch.Stop();
            Console.WriteLine(string.Format("Operation complete in {0} milliseconds", stopwatch.ElapsedMilliseconds));
            Console.ReadKey();
        }

        static void SearchFolder(string folder, string extension, int depth)
        {
            string[] files = Directory.GetFiles(folder);
            bool flag = false;
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == extension)
                {
                    Character character = null; //new Character(files[i], @"C:\Users\Mythic\Desktop\LolFiles\");
                    if (!character.Type.Contains("Structure"))
                        characterFiles.Add(character);
                    if (character.Type.Contains("Champion") ||
                        character.Type.Contains("Minion") ||
                        character.Type.Contains("Ward") ||
                        character.Type.Contains("Special") ||
                        character.Type.Contains("Monster"))
                        flag = true;
                    Console.Title = string.Format("Searching .inibin files... {0} found", characterFiles.Count);
                }
            }

            string[] folders = Directory.GetDirectories(folder);
            if (depth > 0)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    SearchFolder(folders[i], extension, depth - 1);
                }
            }
            if (depth == 0 && flag)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    if (folders[i].Substring(folders[i].LastIndexOf('\\') + 1).ToLower() == "skins")
                    {
                        SearchFolder(folders[i], extension);
                    }
                }
            }
        }

        static void SearchFolder(string folder, string extension)
        {
            string[] files = Directory.GetFiles(folder);
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == extension)
                {
                    characterFiles.Add(new Character(files[i], @"C:\Users\Mythic\Desktop\LolFiles\"));
                    Console.Title = string.Format("Searching .inibin files... {0} found", characterFiles.Count);
                }
            }

            string[] folders = Directory.GetDirectories(folder);
            for (int i = 0; i < folders.Length; i++)
            {
                SearchFolder(folders[i], extension);
            }
        }
    }
}
