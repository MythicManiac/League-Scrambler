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
            //string archivePath1 = @"C:\Games\League of Legends\RADS\projects\lol_game_client\filearchives\0.0.1.11\Archive_2.raf";
            //string archivePath2 = @"TestArchive.raf";
            //string filePath1 = @"debug.png";
            //string filePath2 = @"test.png";

            //ArchiveReader reader = new ArchiveReader();
            //ArchiveWriter writer = new ArchiveWriter();
            //Archive rewritten = reader.ReadArchive(archivePath2);
            //var data = File.ReadAllBytes(filePath1);
            //var offset = writer.WriteData(rewritten, data);
            //File.WriteAllBytes(filePath2, reader.ReadData(rewritten, offset, data.Length));
            //writer.SetDataLength(rewritten, offset);

            var manager = new ArchiveFileManager(LeagueLocations.GetLeaguePath());
            var files = manager.GetAllFilePaths();
            var graphicFiles = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                if(files[i].Split('.').Last() == "dds")
                {
                    graphicFiles.Add(files[i]);
                }
            }

            Console.WriteLine("Press something to begin writing");
            Console.ReadKey();

            for (int i = 0; i < graphicFiles.Count; i++)
            {
                Console.WriteLine("Reading files {0} / {1}", i + 1, files.Length);
                File.WriteAllBytes(@"TestFolder\" + graphicFiles[i].Split('/').Last(), manager.ReadFile(graphicFiles[i], true));
            }
            Console.WriteLine("All files read!");
        }
    }
}
