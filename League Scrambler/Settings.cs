using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueScrambler
{
    public class Settings : Menu
    {
        private Interface _interface;

        public Settings(Interface userInterface)
        {
            _interface = userInterface;
            Options.Add(new SettingsOption(true, "Scramble models", ToggleModels));
            Options.Add(new SettingsOption(true, "Scramble loadingscreen images", ToggleLoadingscreens));
            Options.Add(new SettingsOption(true, "Scramble ability icons", ToggleAbilities));
            Options.Add(new SettingsOption(true, "Scramble minimap icons", ToggleMinimap));
            Options.Add(new SettingsOption(true, "Scramble champion info icons", ToggleSquare));
            Options.Add(new SettingsOption(false, "Scramble item icons", ToggleItems));
            Options.Add(new MenuOption("Back", GoBack));
        }

        public bool this[int index]
        {
            get
            {
                return ((SettingsOption)Options[index]).Enabled;
            }
            set
            {
                ((SettingsOption)Options[index]).Enabled = value;
            }
        }

        private void ToggleModels()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_MODELS]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_MODELS]).Enabled;
        }

        public void ToggleLoadingscreens()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_LOADINGSCREENS]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_LOADINGSCREENS]).Enabled;
        }

        public void ToggleAbilities()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_ABILITIES]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_ABILITIES]).Enabled;
        }

        public void ToggleMinimap()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_MINIMAP]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_MINIMAP]).Enabled;
        }

        public void ToggleSquare()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_SQUARE]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_SQUARE]).Enabled;
        }

        public void ToggleItems()
        {
            ((SettingsOption)Options[(int)Option.SCRAMBLE_ITEMS]).Enabled = !((SettingsOption)Options[(int)Option.SCRAMBLE_ITEMS]).Enabled;
        }

        public void GoBack()
        {
            _interface.ChangeMenuId(0);
        }
    }

    public enum Option
    {
        SCRAMBLE_MODELS = 0,
        SCRAMBLE_LOADINGSCREENS = 1,
        SCRAMBLE_ABILITIES = 2,
        SCRAMBLE_MINIMAP = 3,
        SCRAMBLE_SQUARE = 4,
        SCRAMBLE_ITEMS = 5
    }

    public class SettingsOption : MenuOption
    {
        private bool _enabled;
        private string _name;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; Title = string.Format("{1} - {0}", _name, value ? "Enabled " : "Disabled"); }
        }

        public SettingsOption(bool enabled, string name, Action action) : base(name, action)
        {
            _name = name;
            Enabled = enabled;
        }
    }
}
