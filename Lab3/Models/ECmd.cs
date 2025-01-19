namespace Lab3.Models
{
    /// <summary>
    /// Перечисление для команд постфиксной записи, например:
    /// ADD, SUB, SET, JZ, JMP, AND, OR, CMPL, CMPE, ...
    /// </summary>
    public enum ECmd
    {
        JMP,
        JZ,
        SET,
        ADD,
        SUB,
        AND,
        OR,
        CMPE,
        CMPNE,
        CMPL,
        CMPLE,
        // если нужно:
        MUL,      // или ADD/SUB/MUL/DIV (если захотите)
        // ...
    }
}
