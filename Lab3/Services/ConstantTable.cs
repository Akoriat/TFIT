using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3.Services
{
    public class ConstantTable
    {
        private List<string> _consts = new List<string>();
        public int AddConstant(string val)
        {
            int idx = _consts.IndexOf(val);
            if (idx < 0)
            {
                _consts.Add(val);
                idx = _consts.Count - 1;
            }
            return idx;
        }
        public string GetConstant(int index) => (index >= 0 && index < _consts.Count) ? _consts[index] : null;
        public IReadOnlyList<string> All => _consts;
    }
}
