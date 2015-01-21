using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using League.Utils;
using League.Files;
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
            string archivePath = @"C:\Games\League of Legends\RADS\projects\lol_game_client\filearchives\0.0.1.11\Archive_2.raf";
            string archivePath2 = @"TestArchive.raf";

            ArchiveReader reader = new ArchiveReader();
            ArchiveWriter writer = new ArchiveWriter();
            Archive original = reader.ReadArchive(archivePath);
            writer.WriteArchive(original, archivePath2);
            Archive rewritten = reader.ReadArchive(archivePath2);

            foreach (var kvp in rewritten.Files)
            {
                if (rewritten.Files[kvp.Key].DataLength != kvp.Value.DataLength)
                {
                    Console.WriteLine("Non matching data {0}", kvp.Key);
                }
            }
        }
    }
}
