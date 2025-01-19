using System.Collections.Generic;

namespace Lab3.Services
{
    public class IdentifierTable
    {
        private List<string> _ids = new List<string>();
        public int AddIdentifier(string name)
        {
            int idx = _ids.IndexOf(name);
            if (idx < 0)
            {
                _ids.Add(name);
                idx = _ids.Count - 1;
            }
            return idx;
        }
        public string GetIdentifier(int index) => (index >= 0 && index < _ids.Count) ? _ids[index] : null;
        public IReadOnlyList<string> All => _ids;
    }
}
