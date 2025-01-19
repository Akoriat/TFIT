namespace Lab4.Models
{
    /// <summary>
    /// Перечисление команд (операций) для ПОЛИЗ и интерпретатора.
    /// Дополните, если нужно больше (MUL, DIV, CMPG, CMPGE, NOT, INP, OUT, ...).
    /// </summary>
    public enum ECmd
    {
        // Управление потоком
        JMP,
        JZ,

        // Присваивание
        SET,

        // Арифметические
        ADD,
        SUB,
        MUL,
        DIV,

        // Логические
        AND,
        OR,
        NOT,

        // Сравнения
        CMPE,   // ==
        CMPNE,  // <>
        CMPL,   // <
        CMPLE,  // <=
        // CMPG,  // >
        // CMPGE, // >=

        // Ввод-вывод (если нужно)
        // INP,
        // OUT,
    }
}
