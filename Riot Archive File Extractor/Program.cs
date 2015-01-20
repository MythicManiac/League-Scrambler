using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using League.Utils;

namespace Riot_Archive_File_Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetOut(new Log("Output.txt"));
            Console.Title = "RAF Manager";

            Extractor extractor = new Extractor("C:\\Games\\League of Legends\\RADS\\projects\\lol_game_client\\filearchives\\0.0.0.235\\", "C:\\Users\\Mythic\\Desktop\\LolTest\\", 2);
            extractor.Extract();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
