namespace Lab3.Models
{
    /// <summary>
    /// Набор команд (операций) ПОЛИЗа, соответствующих
    /// JMP, JZ, SET, ADD, SUB, AND, OR, NOT, CMPE, CMPNE, CMPL, CMPLE, ...
    /// </summary>
    public enum ECmd
    {
        // Переходы
        JMP,
        JZ,

        // Присваивание
        SET,

        // Арифметика
        ADD,
        SUB,

        // Логика
        AND,
        OR,
        NOT,

        // Сравнения
        CMPE,   // ==
        CMPNE,  // <>
        CMPL,   // <
        CMPLE   // <=
    }
}
