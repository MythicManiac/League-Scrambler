using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using League.Utils;

namespace LeagueScrambler
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "League Scrambler";

            // Search for League of Legends installation path from registry
            string leaguePath = LeagueLocations.GetLeaguePath();

            // Make sure the path is valid, if not, ask for user to select it manually. Keep repeating until user exits or selects a proper file.
            while (string.IsNullOrEmpty(leaguePath) || !Directory.Exists(leaguePath))
            {
                MessageBox.Show("Couldn't automatically detect your League of Legends installation path, please select it manually.", "Error", MessageBoxButtons.OK);
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "League of Legends Installation Path";
                dialog.Filter = "Leagu of Legends Launcher|lol.launcher.exe";
                dialog.FilterIndex = 0;
                dialog.Multiselect = false;
                dialog.AutoUpgradeEnabled = true;
                DialogResult result = dialog.ShowDialog();

                if(result == DialogResult.OK)
                {
                    // We only want the directory name. Also add the backslash to keep it consistent with what we get from the registry automatically.
                    leaguePath = Path.GetDirectoryName(dialog.FileName) + "\\";
                }
                else
                {
                    // Ask the user if he'd like to exit since he didn't select a file.
                    result = MessageBox.Show("No file was selected, would you like to exit?", "Error", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        return;
                }
            }

            if (!Directory.Exists(LeagueLocations.GetModPath(leaguePath)))
                Directory.CreateDirectory(LeagueLocations.GetModPath(leaguePath));

            var log = new Log(LeagueLocations.GetModPath(leaguePath) + "Log.txt");
            log.LogLine(new string('#', 100));
            log.LogLine("NEW SESSION STARTED");
            log.LogLine(new string('#', 100));
            Console.SetOut(log);

            // Launch the interface

            try
            {
                Interface ui = new Interface(leaguePath);
                ui.Initialize();
                ui.Run();
            }
            catch(Exception e)
            {
                log.LogLine(new string('#', 50));
                log.LogLine("ERROR OCCURRED");
                log.LogLine(new string('#', 50));
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                log.LogLine(new string('#', 50));
                Console.WriteLine("An error has occurred and it has been logged. Please refer to the troubleshooting section found at https://github.com/MythicManiac/League-of-Legends");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
