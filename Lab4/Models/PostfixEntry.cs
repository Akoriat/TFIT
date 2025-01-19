namespace Lab4.Models
{
    /// <summary>
    /// Один элемент ПОЛИЗ (type + index).
    /// </summary>
    public struct PostfixEntry
    {
        public EEntryType type;
        public int index;

        public PostfixEntry(EEntryType t, int i)
        {
            type = t;
            index = i;
        }
    }
}
