using System.Collections.Generic;

namespace Lab1.Services
{
    public class IdentifierTable
    {
        private readonly List<string> _identifiers = new List<string>();

        public int AddIdentifier(string name)
        {
            int index = _identifiers.IndexOf(name);
            if (index < 0)
            {
                _identifiers.Add(name);
                index = _identifiers.Count - 1;
            }
            return index;
        }

        public string GetIdentifier(int index)
        {
            if (index >= 0 && index < _identifiers.Count)
                return _identifiers[index];
            return null;
        }

        public IReadOnlyList<string> All => _identifiers;
    }
}
