using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using League.Utils;

namespace Inibin_File_Manager
{
    class Program
    {
        static List<string> inibinFiles;

        static void Main(string[] args)
        {
            Console.SetOut(new Log("Output.txt"));

            inibinFiles = new List<string>();

            Console.Title = string.Format("Searching .inibin files... {0} found", inibinFiles.Count);

            //SearchFolder(@"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\", ".inibin", 1);
            SearchFolder(@"C:\Users\Mythic\Desktop\LolFiles\DATA\Characters\", ".inibin", 1);

            Console.WriteLine(string.Format("Inibin files found: {0}", inibinFiles.Count));

            HashFinder hashFinder = new HashFinder(inibinFiles.ToArray(), @"C:\Users\Mythic\Desktop\LolFiles\");
            hashFinder.Process();
            hashFinder.Dump();

            Console.WriteLine("All done");
            Console.Title = "All done";

            Console.ReadKey();
        }

        static void SearchFolder(string folder, string extension, int depth)
        {
            string[] files = Directory.GetFiles(folder);
            bool flag = false;

            for(int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == extension)
                {
                    League.Files.Inibin inibin = new League.Files.Inibin(files[i], @"C:\Users\Mythic\Desktop\LolFiles\");
                    League.Character character = new League.Character(inibin);
                    if(!character.Type.Contains("Structure"))
                        inibinFiles.Add(files[i]);
                    if (character.Type.Contains("Champion") ||
                        character.Type.Contains("Minion") ||
                        character.Type.Contains("Ward") ||
                        character.Type.Contains("Special") ||
                        character.Type.Contains("Monster"))
                        flag = true;

                    Console.Title = string.Format("Searching .inibin files... {0} found", inibinFiles.Count);
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
            if(depth == 0 && flag)
            {
                for(int i = 0; i < folders.Length; i++)
                {
                    if(folders[i].Substring(folders[i].LastIndexOf('\\') + 1).ToLower() == "skins")
                    {
                        Console.WriteLine("Search extended to {0}", folders[i]);
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
                    inibinFiles.Add(files[i]);
                    Console.Title = string.Format("Searching .inibin files... {0} found", inibinFiles.Count);
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
