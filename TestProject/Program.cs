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
using Ionic.Zlib;

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

            var xinOriginal = manager.ReadFile("DATA/Characters/XinZhao/XinZhaoLoadScreen.dds", true);
            var luxOriginal = manager.ReadFile("DATA/Characters/Lux/LuxLoadScreen.dds", true);

            if (manager.ArchivesModified)
                manager.Revert();

            var xin = manager.ReadFile("DATA/Characters/XinZhao/XinZhaoLoadScreen.dds", true);
            var lux = manager.ReadFile("DATA/Characters/Lux/LuxLoadScreen.dds", true);

            manager.BeginWriting();

            manager.WriteFile("DATA/Characters/XinZhao/XinZhaoLoadScreen.dds", true, lux);
            manager.WriteFile("DATA/Characters/Lux/LuxLoadScreen.dds", true, xin);

            manager.EndWriting();

            File.WriteAllBytes(@"C:\Xin.dds", xin.Uncompress());
            File.WriteAllBytes(@"C:\Lux.dds", lux.Uncompress());
            File.WriteAllBytes(@"C:\XinOriginal.dds", xinOriginal.Uncompress());
            File.WriteAllBytes(@"C:\LuxOriginal.dds", luxOriginal.Uncompress());
        }
    }
}
