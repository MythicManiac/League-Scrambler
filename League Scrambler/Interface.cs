using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using League.Utils;
using League.Tools;

namespace LeagueScrambler
{
    public class Interface
    {
        private bool _run;
        private int _menuId;
        private List<Menu> _menus;
        private string _leaguePath;

        public Interface(string leaguePath) { _leaguePath = leaguePath; }

        public void Initialize()
        {
            _menus = new List<Menu>();
            Menu menu = new Menu();
            menu.Options.Add(new MenuOption("Backup files", Backup));
            menu.Options.Add(new MenuOption("Scramble files", Scramble));
            menu.Options.Add(new MenuOption("Restore files (Work changes backwards)", Restore));
            menu.Options.Add(new MenuOption("Restore files from backup (Copy backed up files over the current ones)", RestoreBackup));
            menu.Options.Add(new MenuOption("Exit", Exit));
            _menus.Add(menu);
            _menuId = 0;
        }

        public void Run()
        {
            _run = true;
            var input = -1;
            var message = "";
            while (_run)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select an option");
                for (int i = 0; i < _menus[_menuId].Options.Count; i++)
                {
                    Console.WriteLine("{0} - {1}", i + 1, _menus[_menuId].Options[i].Title);
                }

                try { input = Convert.ToInt32(Console.ReadLine()) - 1; }
                catch { input = -1; }
                
                if(input > -1 && input < _menus[_menuId].Options.Count)
                {
                    _menus[_menuId].Options[input].Action();
                }
                else
                {
                    message = "Invalid input";
                }
            }
        }

        public void Backup()
        {
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(_leaguePath), LeagueLocations.GetManifestPath(_leaguePath), LeagueLocations.GetBackupPath(_leaguePath));
            patcher.Backup(true);
            Console.Title = "League Scrambler";
        }

        public void Scramble()
        {
            Console.Clear();
            Console.WriteLine("Scrambling files");
            Scrambler scrambler = new Scrambler(_leaguePath);
            scrambler.Initialize();
            scrambler.Scramble();
            scrambler.Prepare();
            scrambler.Patch();
            Console.Title = "League Scrambler";
            Console.WriteLine("Done");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        public void Restore()
        {

        }

        public void RestoreBackup()
        {
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(_leaguePath), LeagueLocations.GetManifestPath(_leaguePath), LeagueLocations.GetBackupPath(_leaguePath));
            patcher.Restore(false);
            Console.Title = "League Scrambler";
        }

        public void Exit()
        {
            _run = false;
        }
    }
}
