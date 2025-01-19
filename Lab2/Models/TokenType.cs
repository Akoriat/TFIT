namespace Lab2.Models
{
    public enum TokenType
    {
        // Пример: ключевые слова
        While, Do, End,
        And, Or, Not, Until, Loop, Output,

        // Операции
        Rel,  // <, >, =, <>
        As,   // присваивание
        Ao,   // +, - или * , /

        // Идентификаторы / Константы
        Var,
        Const,

        // Прочее
        Unknown
    }
}
