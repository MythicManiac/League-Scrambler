using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueCommon.Utils
{
    public class Table
    {
        public List<TableRow> Rows { get; private set; }
        public int Columns { get; private set; }

        public Table(int columns)
        {
            Columns = columns;
            Rows = new List<TableRow>();
        }

        public Table(int columns, int rows)
        {
            Columns = columns;
            Rows = new List<TableRow>(rows);
        }

        public void AddRow(params object[] items)
        {
            if (items.Length > Columns)
                throw new Exception("Param count must be less or equal to the table's column amount");

            string[] row = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                row[i] = Convert.ToString(items[i]);
            }
            Rows.Add(new TableRow(row));
        }

        public void AddColumnlessRow(string row)
        {
            Rows.Add(new TableRow(new string[1] { row }, false));
        }

        public void DumpTable(int minPadding = 4)
        {
            int[] lenghts = new int[Columns];
            
            for(int i = 0; i < Rows.Count; i++)
            {
                for(int j = 0; j < Rows[i].Length; j++)
                {
                    if(lenghts[j] < Rows[i][j].Length && Rows[i].Column)
                    {
                        lenghts[j] = Rows[i][j].Length;
                    }
                }
            }

            for(int i = 0; i < Rows.Count; i++)
            {
                string line = "";

                if (Rows[i].Column)
                {
                    for (int j = 0; j < Rows[i].Length; j++)
                    {
                        if (j == Columns - 1)
                            line += Rows[i][j];
                        else
                            line += string.Format("{0}{1}", Rows[i][j], new string(' ', lenghts[j] + minPadding - Rows[i][j].Length));
                    }
                }
                else
                    line = Rows[i][0];

                Console.WriteLine(line);
            }
        }
    }

    public class TableRow
    {
        public string[] Items { get; set; }
        public bool Column { get; set; }

        public TableRow(string[] items, bool column = true)
        {
            Items = items;
            Column = column;
        }

        public int Length
        {
            get
            {
                return Items.Length;
            }
        }

        public string this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }
    }
}
