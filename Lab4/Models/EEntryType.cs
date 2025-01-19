namespace Lab4.Models
{
    public enum EEntryType
    {
        etCmd,      // ECmd 
        etVar,      // индекс переменной (для SET)
        etConst,    // индекс константы (для чтения числа)
        etCmdPtr    // адрес/числовое значение для Jump или LOAD
    }
}
