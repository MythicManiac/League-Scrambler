using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using League.Utils;

namespace LeagueScrambler
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Setup console properly
            Console.SetOut(new Log("Output.txt"));
            Console.Title = "League Scrambler";

            // Search for League of Legends installation path from registry
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string leaguePath = FindInstallataionLocation(regKey, "League of Legends");

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

            // Launch the interface
            Interface ui = new Interface(leaguePath);
            ui.Initialize();
            ui.Run();
        }

        static string FindInstallataionLocation(RegistryKey parentKey, string name)
        {
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey key =  parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (key.GetValue("DisplayName").ToString() == name)
                    {
                        return key.GetValue("InstallLocation").ToString();
                    }
                }
                catch { }
            }
            return "";
        }
    }
}
