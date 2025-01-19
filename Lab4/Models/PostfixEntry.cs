namespace Lab4.Models
{
    /// <summary>
    /// Один элемент ПОЛИЗа.
    /// В зависимости от type, index интерпретируется по-разному.
    /// </summary>
    public struct PostfixEntry
    {
        public EEntryType type;  // etCmd / etVar / etConst / etCmdPtr
        public int index;        // код команды / индекс var / индекс const / адрес

        public PostfixEntry(EEntryType t, int i)
        {
            type = t;
            index = i;
        }
    }
}
