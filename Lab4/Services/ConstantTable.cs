using System.Collections.Generic;

namespace Lab4.Services
{
    /// <summary>
    /// Таблица констант. Аналогична таблице переменных.
    /// </summary>
    public class ConstantTable
    {
        private List<string> _consts = new List<string>();

        public int AddConstant(string value)
        {
            int idx = _consts.IndexOf(value);
            if (idx < 0)
            {
                _consts.Add(value);
                idx = _consts.Count - 1;
            }
            return idx;
        }

        public string GetConstant(int index)
        {
            if (index < 0 || index >= _consts.Count)
                return null;
            return _consts[index];
        }

        public IReadOnlyList<string> All => _consts;
    }
}
