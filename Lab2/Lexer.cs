using System;
using System.Collections.Generic;
using System.Text;

namespace Lab2
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

    // Класс Token описывает отдельную лексему с типом, текстом и позицией во входном потоке
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Position { get; set; }

        public Token(TokenType type, string lexeme, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Position = position;
        }

        public override string ToString() => $"{Lexeme} ({Type})";
    }

    // Простейший лексический анализатор (Lexer)
    public static class Lexer
    {
        public static List<Token> Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();
            int pos = 0;
            while (pos < input.Length)
            {
                // Пропускаем пробельные символы
                if (char.IsWhiteSpace(input[pos]))
                {
                    pos++;
                    continue;
                }

                int start = pos;
                char c = input[pos];

                // Если символ буква – читаем слово (идентификатор или ключевое слово)
                if (char.IsLetter(c))
                {
                    StringBuilder sb = new StringBuilder();
                    while (pos < input.Length && (char.IsLetterOrDigit(input[pos]) || input[pos] == '_'))
                    {
                        sb.Append(input[pos]);
                        pos++;
                    }
                    string word = sb.ToString();
                    switch (word.ToLower())
                    {
                        case "do":
                            tokens.Add(new Token(TokenType.lDo, word, start));
                            break;
                        case "until":
                            tokens.Add(new Token(TokenType.lUntil, word, start));
                            break;
                        case "loop":
                            tokens.Add(new Token(TokenType.lLoop, word, start));
                            break;
                        case "not":
                            tokens.Add(new Token(TokenType.lNot, word, start));
                            break;
                        case "and":
                            tokens.Add(new Token(TokenType.lAnd, word, start));
                            break;
                        case "or":
                            tokens.Add(new Token(TokenType.lOr, word, start));
                            break;
                        case "output":
                            tokens.Add(new Token(TokenType.lOutput, word, start));
                            break;
                        default:
                            // Если слово не является ключевым – идентификатор
                            tokens.Add(new Token(TokenType.lVar, word, start));
                            break;
                    }
                }
                // Если цифра – читаем число (константу)
                else if (char.IsDigit(c))
                {
                    StringBuilder sb = new StringBuilder();
                    while (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        sb.Append(input[pos]);
                        pos++;
                    }
                    tokens.Add(new Token(TokenType.lConst, sb.ToString(), start));
                }
                else
                {
                    // Обработка операторов и разделителей
                    if (c == '<')
                    {
                        if (pos + 1 < input.Length && input[pos + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.lNotEqual, "<>", start));
                            pos += 2;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.lLess, "<", start));
                            pos++;
                        }
                    }
                    else if (c == '=')
                    {
                        tokens.Add(new Token(TokenType.lEqual, "=", start));
                        pos++;
                    }
                    else if (c == '>')
                    {
                        tokens.Add(new Token(TokenType.lGreater, ">", start));
                        pos++;
                    }
                    else if (c == '+')
                    {
                        tokens.Add(new Token(TokenType.lPlus, "+", start));
                        pos++;
                    }
                    else if (c == '-')
                    {
                        tokens.Add(new Token(TokenType.lMinus, "-", start));
                        pos++;
                    }
                    else if (c == '*')
                    {
                        tokens.Add(new Token(TokenType.lMul, "*", start));
                        pos++;
                    }
                    else if (c == '/')
                    {
                        tokens.Add(new Token(TokenType.lDiv, "/", start));
                        pos++;
                    }
                    else if (c == ';')
                    {
                        tokens.Add(new Token(TokenType.lSemicolon, ";", start));
                        pos++;
                    }
                    else
                    {
                        // Пропускаем неизвестный символ
                        pos++;
                    }
                }
            }
            return tokens;
        }
    }
}
