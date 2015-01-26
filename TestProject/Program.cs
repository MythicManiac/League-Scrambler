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

            var types = new string[5] { "Champion", "Minion", "Monster", "Ward", "Special" };
            var characters = search.FindCharacters(types);

            var table = new Table(4);

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].LoadSkins(search);

                for (int j = 0; j < characters[i].Skins.Length; j++)
                {
                    table.AddRow(characters[i].Skins[j].BlndFile, characters[i].Skins[j].DdsFile, characters[i].Skins[j].SklFile, characters[i].Skins[j].SknFile);
                }
            }

            table.Dump();
        }
    }
}
