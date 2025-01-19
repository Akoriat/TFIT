namespace Lab3.Models
{
    /// <summary>
    /// Один элемент в постфиксной записи.
    /// В зависимости от type поле index интерпретируется по-разному.
    /// </summary>
    public struct PostfixEntry
    {
        public EEntryType type;  // etCmd, etVar, etConst, etCmdPtr
        public int index;        // индекс команды (ECmd), или индекс var/const, или адрес

        public PostfixEntry(EEntryType t, int i)
        {
            type = t;
            index = i;
        }
    }
}
