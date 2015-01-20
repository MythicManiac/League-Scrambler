using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueCommon.Files;

namespace Inibin_File_Manager
{
    public class HashFinder
    {
        public Inibin[] InibinFiles { get; private set; }
        public Dictionary<uint, List<string>> Data { get; private set; }

        private int _longestName = 0;

        public HashFinder(string[] files, string root)
        {
            InibinFiles = new Inibin[files.Length];
            for(int i = 0; i < files.Length; i++)
            {
                InibinFiles[i] = new Inibin(files[i], root);
                if (InibinFiles[i].LeaguePath.Length > _longestName)
                    _longestName = InibinFiles[i].LeaguePath.Length;
            }
        }

        public void Process()
        {
            Data = new Dictionary<uint, List<string>>();
         
            for (int i = 0; i < InibinFiles.Length; i++)
            {
                Console.Title = string.Format("Processing inibin files... {0} / {1}", i + 1, InibinFiles.Length);
                if (InibinFiles[i].Read())
                {
                    foreach (KeyValuePair<uint, object> kvp in InibinFiles[i].Data)
                    {
                        if (!Data.ContainsKey(kvp.Key))
                            Data[kvp.Key] = new List<string>();

                        if (kvp.Value.GetType() == typeof(string)/*&& ((string)kvp.Value).Contains('.')*/)
                        {
                            string ext = ((string)kvp.Value).Split('.').Last();

                            //if(/*ext == "skl" || */ext == "dds" /*|| ext == "skn" ||*/ /*ext == "blnd"*/)
                            Data[kvp.Key].Add(string.Format("{0}{1}{2}", InibinFiles[i].LeaguePath, new string(' ', _longestName + 4 - InibinFiles[i].LeaguePath.Length), kvp.Value));
                        }
                    }
                }
            }

            Console.WriteLine(string.Format("Unique hashesh found: {0}", Data.Count));
        }

        public void Dump()
        {
            int position = 1;
            foreach (KeyValuePair<uint, List<string>> kvp in Data)
            {
                Console.Title = string.Format("Dumping data... {0} / {1}", position, Data.Count);
                if (kvp.Value.Count > 0)
                {
                    Console.WriteLine(kvp.Key);
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        Console.WriteLine(string.Format("    {0}", kvp.Value[i]));
                    }
                }
                position++;
            }
        }
    }
}
