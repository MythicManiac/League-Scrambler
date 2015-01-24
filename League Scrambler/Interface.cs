using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        private string _message;
        private Settings _settings;

        private ArchiveFileManager _manager;

        public Interface(string leaguePath) { _leaguePath = leaguePath; }

        public void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Initializing...");

            _menus = new List<Menu>();
            Menu menu = new Menu();
            menu.Options.Add(new MenuOption("Backup files", Backup));
            menu.Options.Add(new MenuOption("Scramble files", Scramble));
            menu.Options.Add(new MenuOption("Restore files (Work changes backwards)", Restore));
            menu.Options.Add(new MenuOption("Restore files from backup (Copy backed up files over the current ones)", RestoreBackup));
            menu.Options.Add(new MenuOption("Options", OpenSettings));
            menu.Options.Add(new MenuOption("Exit", Exit));
            _menus.Add(menu);
            _settings = new Settings(this);
            _menus.Add(_settings);
            _menuId = 0;
            _message = "";
            _manager = new ArchiveFileManager(_leaguePath);
        }

        public void Run()
        {
            _run = true;
            var input = -1;
            while (_run)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(_message);
                _message = "";
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select an option");
                for (int i = 0; i < _menus[_menuId].Options.Count; i++)
                {
                    Console.WriteLine("{0} - {1}", i + 1, _menus[_menuId].Options[i].Title);
                }

                try { input = Convert.ToInt32(Console.ReadKey().KeyChar.ToString()) - 1; }
                catch { input = -1; }
                
                if(input > -1 && input < _menus[_menuId].Options.Count)
                {
                    _menus[_menuId].Options[input].Action();
                }
                else
                {
                    _message = "Invalid input";
                }
            }
        }

        public void ChangeMenuId(int id)
        {
            _menuId = id;
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
            scrambler.Scramble(_settings);
            scrambler.Patch(_manager);
            Console.Title = "League Scrambler";
            Console.WriteLine("Done");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        public void Restore()
        {
            Console.Clear();
            Console.WriteLine("Reverting changes...");
            _manager.Revert();
            _message = "Changes reverted";
        }

        public void RestoreBackup()
        {
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(_leaguePath), LeagueLocations.GetManifestPath(_leaguePath), LeagueLocations.GetBackupPath(_leaguePath));
            patcher.Restore(false);
            
            // Remove the corrupt data flag if it exists
            if (File.Exists(LeagueLocations.GetCorruptFlagPath(_leaguePath)))
                File.Delete(LeagueLocations.GetCorruptFlagPath(_leaguePath));

            Console.Title = "League Scrambler";
        }

        public void OpenSettings()
        {
            _menuId = 1;
        }

        public void Exit()
        {
            _run = false;
        }
    }
}
