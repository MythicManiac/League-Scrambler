using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueScrambler
{
    public class Menu
    {
        public List<MenuOption> Options { get; set; }

        public Menu() { Options = new List<MenuOption>(); }
        public Menu(List<MenuOption> options) { Options = options; }
    }
}
