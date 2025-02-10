namespace Lab2.Models
{
    // Перечисление типов токенов (лексем)
    public enum TokenType
    {
        lDo,         // "do"
        lUntil,      // "until"
        lLoop,       // "loop"
        lNot,        // "not"
        lAnd,        // "and"
        lOr,         // "or"
        lOutput,     // "output"
        lVar,        // идентификатор (переменная)
        lConst,      // константа
        lEqual,      // "=" (оператор сравнения/присваивания)
        lLess,       // "<"
        lGreater,    // ">"
        lNotEqual,   // "<>"
        lPlus,       // "+"
        lMinus,      // "-"
        lMul,        // "*"
        lDiv,        // "/"
        lSemicolon   // ";"
    }
}
