using System;
using System.Collections.Generic;
using System.Text;
using Lab4.Models;

namespace Lab4.Services
{
    public class Lexer
    {
        public List<Token> Analyze(string text)
        {
            var tokens = new List<Token>();
            int position = 0;

            while (position < text.Length)
            {
                char c = text[position];

                // Пропуски
                if (char.IsWhiteSpace(c))
                {
                    position++;
                    continue;
                }

                // Идентификатор/ключевое слово
                if (char.IsLetter(c))
                {
                    int startPos = position;
                    while (position < text.Length && (char.IsLetterOrDigit(text[position])))
                    {
                        position++;
                    }
                    string lexeme = text.Substring(startPos, position - startPos);
                    TokenType type = CheckKeyword(lexeme);
                    tokens.Add(new Token(type, lexeme, startPos));
                    continue;
                }

                // Число (константа)
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

                // Операции сравнения / присваивания
                if (c == '<' || c == '>')
                {
                    int startPos = position;
                    position++;
                    if (position < text.Length)
                    {
                        char next = text[position];
                        // "<=", "<>", ">=", и т.д.
                        if (next == '=' || (c == '<' && next == '>'))
                        {
                            position++;
                            string op = c.ToString() + next.ToString();
                            tokens.Add(new Token(TokenType.Rel, op, startPos));
                            continue;
                        }
                    }
                    // иначе просто "<" или ">"
                    tokens.Add(new Token(TokenType.Rel, c.ToString(), startPos));
                    continue;
                }

                if (c == '=')
                {
                    int startPos = position;
                    position++;
                    // Проверить "=="
                    if (position < text.Length && text[position] == '=')
                    {
                        position++;
                        tokens.Add(new Token(TokenType.Rel, "==", startPos));
                    }
                    else
                    {
                        // Считаем это оператором присваивания (as)
                        tokens.Add(new Token(TokenType.As, "=", startPos));
                    }
                    continue;
                }

                // + или -
                if (c == '+' || c == '-')
                {
                    tokens.Add(new Token(TokenType.Ao, c.ToString(), position));
                    position++;
                    continue;
                }

                // Если что-то неизвестное
                tokens.Add(new Token(TokenType.Unknown, c.ToString(), position));
                position++;
            }

            return tokens;
        }

        private TokenType CheckKeyword(string lexeme)
        {
            switch (lexeme.ToLower())
            {
                case "while": return TokenType.While;
                case "end": return TokenType.End;
                case "and": return TokenType.And;
                case "or": return TokenType.Or;
                case "not": return TokenType.Not;
                case "do": return TokenType.Do;
                case "until": return TokenType.Until;
                case "loop": return TokenType.Loop;
                case "as": return TokenType.As;

                default: return TokenType.Var;
            }
        }
    }
}
