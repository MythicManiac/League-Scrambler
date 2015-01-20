using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using LeagueCommon.Files;

namespace Riot_Archive_File_Extractor
{
    public class Extractor
    {
        public List<Archive> Archives { get; private set; }
        public string SourceDirectory { get; private set; }
        public string DestinationDirectory { get; private set; }
        public int ThreadCount { get; private set; }

        private Thread[] _threads;
        private int[] _status;
        private List<int>[] _threadArchives;

        public Extractor(string sourceDirectory, string destinationDirectory, int threadCount)
        {
            SourceDirectory = sourceDirectory;
            DestinationDirectory = destinationDirectory;
            ThreadCount = threadCount;
        }

        public void Extract()
        {
            Archives = new List<Archive>();
            ScanFiles(SourceDirectory);
            PrepareThreads();

            int totalStatus = 0;
            while(totalStatus < Archives.Count)
            {
                totalStatus = 0;
                for (int i = 0; i < _status.Length; i++)
                {
                    totalStatus += _status[i];
                }
                Console.Title = string.Format("Extracting... - Progress {0} / {1}", totalStatus, Archives.Count);
                Thread.Sleep(100);
            }

            Console.WriteLine("Extraction complete!");
        }

        private void ScanFiles(string path)
        {
            // Check if there are any .raf files inside this folder
            string[] files = Directory.GetFiles(path);
            for(int i = 0; i < files.Length; i++)
            {
                if(Path.GetExtension(files[i]) == ".raf" && File.Exists(files[i] + ".dat"))
                {
                    Archives.Add(new Archive(files[i]));
                }
            }

            // Scan all the subdirectories as well
            string[] directories = Directory.GetDirectories(path);
            for(int i = 0; i < directories.Length; i++)
            {
                ScanFiles(directories[i]);
            }
        }

        private void PrepareThreads()
        {
            _threads = new Thread[ThreadCount];
            _status = new int[ThreadCount];
            _threadArchives = new List<int>[ThreadCount];

            for (int i = 0; i < _threadArchives.Length; i++)
            {
                _threadArchives[i] = new List<int>();
            }

            for (int i = 0; i < Archives.Count; i++)
            {
                _threadArchives[i % ThreadCount].Add(i);
            }

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(new ParameterizedThreadStart(ExtractionThread));
                _threads[i].Start(i);
            }
        }

        private void ExtractionThread(object input)
        {
            int id = (int)input;
            for(int i = 0; i < _threadArchives[id].Count; i++)
            {
                Archives[_threadArchives[id][i]].Read();
                Archives[_threadArchives[id][i]].Extract(DestinationDirectory);
                _status[id]++;
            }
        }
    }
}
