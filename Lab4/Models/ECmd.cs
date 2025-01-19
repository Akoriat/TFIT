namespace Lab4.Models
{
    /// <summary>
    /// Команды для ПОЛИЗ + интерпретатор
    /// </summary>
    public enum ECmd
    {
        // Новая команда для чтения переменной
        LOAD,

        JMP,
        JZ,
        SET,

        // Арифметические
        ADD,
        SUB,
        // (можно добавить MUL, DIV)

        // Логические
        AND,
        OR,
        NOT,

        // Сравнения
        CMPE,
        CMPNE,
        CMPL,
        CMPLE
    }
}
