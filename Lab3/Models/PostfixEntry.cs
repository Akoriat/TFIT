namespace Lab3.Models
{
    /// <summary>
    /// Один элемент постфиксной записи (type + index).
    /// В зависимости от type, index может быть:
    /// - код команды (ECmd)
    /// - индекс переменной
    /// - индекс константы
    /// - адрес
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
