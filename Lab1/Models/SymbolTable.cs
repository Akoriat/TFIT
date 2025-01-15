using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Models
{
    public class SymbolTable
    {
        private readonly Dictionary<string, int> _table = new();

        public int Add(string value)
        {
            if (!_table.ContainsKey(value))
            {
                int id = _table.Count;
                _table[value] = id;
            }
            return _table[value];
        }

        public Dictionary<string, int> GetTable() => _table;
    }

}
