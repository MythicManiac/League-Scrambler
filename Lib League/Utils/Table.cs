using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Utils
{
    public class Table
    {
        public int Columns { get; private set; }

        private int[] _lengths;
        private List<TableRow> _rows;

        public Table(int columns)
        {
            Columns = columns;
            _rows = new List<TableRow>();
            _lengths = new int[columns];
        }

        public Table(int columns, int rows)
        {
            Columns = columns;
            _rows = new List<TableRow>(rows);
            _lengths = new int[columns];
        }

        public void AddRow(params object[] items)
        {
            if (items.Length > Columns)
                throw new Exception("Param count must be less or equal to the table's column amount");

            string[] row = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                row[i] = Convert.ToString(items[i]);
                if (row[i].Length > _lengths[i])
                    _lengths[i] = row[i].Length;
            }

            _rows.Add(new TableRow(row));
        }

        public void AddColumnlessRow(string row)
        {
            _rows.Add(new TableRow(new string[1] { row }, false));
        }

        public void DumpTable(int minPadding = 4)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                string line = "";

                if (_rows[i].Column)
                {
                    for (int j = 0; j < _rows[i].Length; j++)
                    {
                        if (j == Columns - 1)
                            line += _rows[i][j];
                        else
                            line += string.Format("{0}{1}", _rows[i][j], new string(' ', _lengths[j] + minPadding - _rows[i][j].Length));
                    }
                }
                else
                    line = _rows[i][0];

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
