using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab3.Models;

namespace Lab3.Services
{
    public class Lexer
    {
        /// <summary>
        /// Основной метод: принимает исходный текст, возвращает список лексем.
        /// </summary>
        public List<Token> Analyze(string text)
        {
            List<Token> tokens = new List<Token>();
            int position = 0;

            while (position < text.Length)
            {
                char c = text[position];

                // Пропускаем пробелы/табуляции/переводы строк
                if (char.IsWhiteSpace(c))
                {
                    position++;
                    continue;
                }

                // --------------------------------------
                //  1) Ключевые слова или идентификатор
                // --------------------------------------
                if (char.IsLetter(c))
                {
                    int startPos = position;
                    while (position < text.Length && (char.IsLetterOrDigit(text[position])))
                    {
                        position++;
                    }
                    string lexeme = text.Substring(startPos, position - startPos);

                    // Проверяем, не ключевое ли слово
                    TokenType type = CheckKeyword(lexeme);
                    tokens.Add(new Token(type, lexeme, startPos));
                    continue;
                }

                // --------------------------------------
                //  2) Число (константа)
                // --------------------------------------
                if (char.IsDigit(c))
                {
                    int startPos = position;
                    while (position < text.Length && char.IsDigit(text[position]))
                    {
                        position++;
                    }
                    string lexeme = text.Substring(startPos, position - startPos);
                    tokens.Add(new Token(TokenType.Const, lexeme, startPos));
                    continue;
                }

                // --------------------------------------
                //  3) Сравнительные операции rel (<, >, ==, <>, <=, >=, ...)
                //     или оператор присваивания '=' (as)
                // --------------------------------------

                // Случай: '<' или '>'
                if (c == '<' || c == '>')
                {
                    int startPos = position;
                    position++;
                    // Проверим следующий символ: '=', '>'
                    if (position < text.Length)
                    {
                        char next = text[position];
                        if (next == '=' || (c == '<' && next == '>'))
                        {
                            // "<=" или "<>"
                            // ">=" (если хотим поддерживать)
                            position++;
                            string op = c.ToString() + next.ToString();
                            tokens.Add(new Token(TokenType.Rel, op, startPos));
                            continue;
                        }
                    }
                    // Иначе просто "<" или ">"
                    tokens.Add(new Token(TokenType.Rel, c.ToString(), startPos));
                    continue;
                }

                // Случай: '=' (может быть присваивание или часть ==)
                if (c == '=')
                {
                    int startPos = position;
                    position++;
                    // Проверяем, не идёт ли '=' следом => "=="
                    if (position < text.Length && text[position] == '=')
                    {
                        // "=="
                        position++;
                        tokens.Add(new Token(TokenType.Rel, "==", startPos));
                    }
                    else
                    {
                        // Просто "=" -> считаем это присваиванием (as)
                        tokens.Add(new Token(TokenType.As, "=", startPos));
                    }
                    continue;
                }

                // --------------------------------------
                //  4) Арифметические операции ao (+, -, *, /)
                // --------------------------------------
                if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    tokens.Add(new Token(TokenType.Ao, c.ToString(), position));
                    position++;
                    continue;
                }

                // --------------------------------------
                //  5) Иначе: неизвестный символ
                // --------------------------------------
                tokens.Add(new Token(TokenType.Unknown, c.ToString(), position));
                position++;
            }

            return tokens;
        }

        /// <summary>
        /// Проверяем, не является ли строка ключевым словом (while, do, end, etc.).
        /// Если нет, возвращаем TokenType.Var (т.е. обычный идентификатор).
        /// </summary>
        private TokenType CheckKeyword(string lexeme)
        {
            switch (lexeme.ToLower())
            {
                case "while": return TokenType.While;
                case "do": return TokenType.Do;
                case "end": return TokenType.End;
                case "and": return TokenType.And;
                case "or": return TokenType.Or;
                case "not": return TokenType.Not;
                case "until": return TokenType.Until;
                case "loop": return TokenType.Loop;
                case "output": return TokenType.Output;
                case "as": return TokenType.As;

                default: return TokenType.Var;
            }
        }
    }
}
