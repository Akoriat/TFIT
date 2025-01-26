using System.Collections.Generic;

namespace Lab1.Services
{
    public class ConstantTable
    {
        private readonly List<string> _constants = new List<string>();

        public int AddConstant(string value)
        {
            int index = _constants.IndexOf(value);
            if (index < 0)
            {
                _constants.Add(value);
                index = _constants.Count - 1;
            }
            return index;
        }

        public string GetConstant(int index)
        {
            if (index >= 0 && index < _constants.Count)
                return _constants[index];
            return null;
        }

        public IReadOnlyList<string> All => _constants;
    }
}
