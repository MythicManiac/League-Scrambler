using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueScrambler
{
    public class MenuOption
    {
        public string Title { get; set; }
        public Action Action { get; set; }

        public MenuOption(string title, Action action)
        {
            this.Title = title;
            this.Action = action;
        }
    }
}
