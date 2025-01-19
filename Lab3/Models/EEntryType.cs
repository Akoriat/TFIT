namespace Lab3.Models
{
    /// <summary>
    /// Тип элемента в постфиксной записи (ПОЛИЗ).
    /// </summary>
    public enum EEntryType
    {
        // Команда (операция), напр. ADD, SUB, SET
        etCmd,
        // Переменная (индекс в таблице переменных)
        etVar,
        // Константа (индекс в таблице констант)
        etConst,
        // Адрес команды (индекс элемента в ПОЛИЗ)
        etCmdPtr
    }
}
