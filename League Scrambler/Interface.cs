using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
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

        private ArchiveFileManager _fileManager;
        private BackupManager _backupManager;

        public Interface(string leaguePath) { _leaguePath = leaguePath; }

        public void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Initializing...");

            _menus = new List<Menu>();
            var menu = new Menu();
            menu.Options.Add(new MenuOption("Backup files", Backup));
            menu.Options.Add(new MenuOption("Scramble files", Scramble));
            menu.Options.Add(new MenuOption("Restore files (Work changes backwards)", Restore));
            menu.Options.Add(new MenuOption("Restore files from backup (Copy backed up files over the current ones)", RestoreBackup));
            menu.Options.Add(new MenuOption("Options", OpenSettings));
            menu.Options.Add(new MenuOption("Exit", Exit));
            _menus.Add(menu);

            _settings = new Settings(this);
            _menus.Add(_settings);

            menu = new Menu();
            menu.Options.Add(new MenuOption("Remove old backup and take a new one", ForceBackup));
            menu.Options.Add(new MenuOption("Go back", ReturnToMainMenu));
            _menus.Add(menu);

            _menuId = 0;
            _message = "";
            _fileManager = new ArchiveFileManager(_leaguePath);
            _backupManager = new BackupManager(_leaguePath);
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
            if (!_backupManager.GetBackupState())
            {
                Console.Clear();
                Console.WriteLine("Taking a backup");
                _backupManager.Backup(true);
                _message = "Backup taken";
            }
            else
            {
                _message = "A backup already exists, would you like to overwrite it?";
                _menuId = 2;
            }
        }

        public void ForceBackup()
        {
            Console.Clear();
            Console.WriteLine("Taking a backup");

            _backupManager.Backup(true);
            _menuId = 0;
            _message = "Backup taken";
        }

        public void ReturnToMainMenu()
        {
            _menuId = 0;
        }

        public void Scramble()
        {
            Console.Clear();
            Console.WriteLine("Scrambling files");
            var scrambler = new Scrambler(_leaguePath);

            var ticks = DateTime.Now.Ticks % Int32.MaxValue;
            var seed = Convert.ToInt32(ticks);
            
            var success = false;
            while (!success)
            {
                success = int.TryParse(Interaction.InputBox("Random seed:", "Enter a valid seed (Numbers only)", seed.ToString()), out seed);
            }

            scrambler.Scramble(_settings, seed);
            scrambler.Patch(_fileManager);
            Console.Title = "League Scrambler";
            Console.WriteLine("Done");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        public void Restore()
        {
            Console.Clear();
            Console.WriteLine("Reverting changes...");
            _fileManager.Revert();
            _message = "Changes reverted";
        }

        public void RestoreBackup()
        {
            Console.Clear();
            Console.WriteLine("Restoring backup");

            _backupManager.Restore(false);

            _message = "Backup restored";
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
