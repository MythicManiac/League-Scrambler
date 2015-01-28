using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using League.Utils;
using League.Files;
using League.Tools;
using League;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetOut(new Log("Output.txt"));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Run();
            stopwatch.Stop();
            Console.WriteLine(string.Format("Operation completed in {0} milliseconds", stopwatch.ElapsedMilliseconds));
            Console.ReadKey();
        }

        public static void Run()
        {
            var manager = new ArchiveFileManager(LeagueLocations.GetLeaguePath());
            var search = new ManifestSearch(manager);

            //CompileAssetLists(search);
        }

        public static void CompileAssetLists(ManifestSearch search)
        {
            new SkinGroupWriter().Write(@"C:\SkinGroups.dat", FindSkinGroups(search));
            var writer = new PathListWriter();
            writer.Write(@"C:\LoadScreenPaths.dat", search.FindLoadingScreens());
            writer.Write(@"C:\AbilityIconPaths.dat", search.FindAbilityIcons());
            writer.Write(@"C:\SquareIconPaths.dat", search.FindSquareIcons());
            writer.Write(@"C:\CircleIconPaths.dat", search.FindCircleIcons());
            writer.Write(@"C:\ItemIconPaths.dat", search.FindItemIcons());
        }

        public static List<SkinGroup> FindSkinGroups(ManifestSearch search)
        {
            var types = new string[5] { "Champion", "Minion", "Monster", "Ward", "Special" };
            var characters = search.FindCharacters(types, false);

            var groups = new List<SkinGroup>();

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].LoadSkins(search);

                for (int j = 0; j < characters[i].Skins.Length; j++)
                {
                    var flag = false;
                    for (int k = 0; k < groups.Count; k++)
                    {
                        if (groups[k].Common(characters[i].Skins[j]))
                        {
                            if (!groups[k].Equals(characters[i].Skins[j]))
                                groups[k].Skins.Add(characters[i].Skins[j]);

                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                        groups.Add(new SkinGroup(characters[i].Skins[j]));
                }
            }

            return groups;
        }
    }
}
