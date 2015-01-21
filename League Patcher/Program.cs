using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using League.Utils;
using League.Tools;
using League.Files.Manifest;
using League.Files;

namespace League_Patcher
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
            string lol = @"C:\Games\League of Legends\";
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(lol), LeagueLocations.GetManifestPath(lol), LeagueLocations.GetBackupPath(lol));

            int mode = 5;

            if (mode == 1)
            {
                patcher.Restore(false);
            }
            if (mode == 2)
            {
                patcher.LoadArchives();
                patcher.AddFileChange(@"DATA\Characters\XinZhao\XinZhao.skl", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\Lux\Lux.skl");
                patcher.AddFileChange(@"DATA\Characters\XinZhao\XinZhao.skn", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\Lux\Lux.skn");
                patcher.AddFileChange(@"DATA\Characters\XinZhao\XinZhaoBase.blnd", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\Lux\BaseLux.blnd");
                patcher.AddFileChange(@"DATA\Characters\XinZhao\XenZhao_v2.dds", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\Lux\lux_base_CM_TX.dds");
                patcher.AddFileChange(@"DATA\Characters\XinZhao\XinZhaoLoadScreen.dds", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\Lux\LuxLoadScreen.dds");
                patcher.AddFileChange(@"DATA\Characters\Lux\Lux.skl", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\XinZhao\XinZhao.skl");
                patcher.AddFileChange(@"DATA\Characters\Lux\Lux.skn", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\XinZhao\XinZhao.skn");
                patcher.AddFileChange(@"DATA\Characters\Lux\BaseLux.blnd", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\XinZhao\XinZhaoBase.blnd");
                patcher.AddFileChange(@"DATA\Characters\Lux\lux_base_CM_TX.dds", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\XinZhao\XenZhao_v2.dds");
                patcher.AddFileChange(@"DATA\Characters\Lux\LuxLoadScreen.dds", @"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\XinZhao\XinZhaoLoadScreen.dds");
                patcher.Patch();
            }
            if (mode == 3)
            {
                ReleaseManifest manifest = ReleaseManifest.LoadFromFile(@"C:\Games\League of Legends\RADS\projects\lol_game_client\releases\0.0.1.11\releasemanifest");
                manifest.Dump();
            }
            if(mode == 4)
            {
                patcher.Backup(true);
            }
            if(mode == 5)
            {
                List<string> asd = new List<string>();
                asd.Add("DSA");
                asd.Add("LELELELE");
                asd.Add("HOUSTU");
                asd.Add("PAITA");
                PathListWriter writer = new PathListWriter();
                writer.Write(@"C:\PathList.dat", asd);
                PathListReader reader = new PathListReader();
                string[] paths = reader.Read(@"C:\PathList.dat");
                for (int i = 0; i < paths.Length; i++)
                    Console.WriteLine(paths[i]);
            }
        }

        public static void TestList()
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Generating values for object list");

            ObjectList<GameObject> list = GenItems(1024);

            int testCount = 1000;

            Console.WriteLine("Running benchmark for List");

            for (int i = 0; i < testCount; i++)
            {
                stopwatch.Start();
                list.Reorder();
                stopwatch.Stop();
            }

            Console.WriteLine(string.Format("Benchmark completed in {0} milliseconds, {1} per object", (stopwatch.ElapsedTicks / (float)testCount) / 1000f, ((stopwatch.ElapsedTicks / (float)testCount) / 1000f) / list.Length));
        }

        public static ObjectList<GameObject> GenItems(int halfLength)
        {
            ObjectList<GameObject> list = new ObjectList<GameObject>(halfLength * 2);

            for (int i = 0; i < halfLength * 2; i++)
            {
                list.AddItem(new GameObject(true));
            }
            for (int i = 0; i < halfLength; i++)
            {
                list.KillRandom();
            }

            return list;
        }
    }
}
