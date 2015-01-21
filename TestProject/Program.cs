using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using League.Utils;
using League.Files;

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

            ArchiveReader reader = new ArchiveReader();
            Archive archive = reader.ReadArchive(archivePath);

            Table table = new Table(1);
            foreach(KeyValuePair<string, ArchiveFileInfo> kvp in archive.FileList)
            {
                table.AddRow(kvp.Key);
            }
            table.Dump();
        }
    }
}
