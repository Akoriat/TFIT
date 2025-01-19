using System;
using System.Collections.Generic;
using Lab3.Models;

namespace Lab3.Services
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

                // Пропускаем пробелы, табуляцию и т.д.
                if (char.IsWhiteSpace(c))
                {
                    position++;
                    continue;
                }

                // Идентификатор/ключевое слово
                if (char.IsLetter(c))
                {
                    int startPos = position;
                    while (position < text.Length && char.IsLetterOrDigit(text[position]))
                    {
                        position++;
                    }
                    string lexeme = text.Substring(startPos, position - startPos);
                    TokenType ttype = CheckKeyword(lexeme);
                    tokens.Add(new Token(ttype, lexeme, startPos));
                    continue;
                }

                // Число
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

                // <, >
                if (c == '<' || c == '>')
                {
                    int startPos = position;
                    position++;
                    if (position < text.Length)
                    {
                        char next = text[position];
                        if (next == '=' || (c == '<' && next == '>'))
                        {
                            position++;
                            string op = c.ToString() + next.ToString(); // "<=", "<>"
                            tokens.Add(new Token(TokenType.Rel, op, startPos));
                            continue;
                        }
                    }
                    // иначе просто < или >
                    tokens.Add(new Token(TokenType.Rel, c.ToString(), startPos));
                    continue;
                }

                // =
                if (c == '=')
                {
                    int startPos = position;
                    position++;
                    // Проверяем "=="
                    if (position < text.Length && text[position] == '=')
                    {
                        position++;
                        tokens.Add(new Token(TokenType.Rel, "==", startPos));
                    }
                    else
                    {
                        // одиночный '=' => As
                        tokens.Add(new Token(TokenType.As, "=", startPos));
                    }
                    continue;
                }

                // +, -
                if (c == '+' || c == '-')
                {
                    tokens.Add(new Token(TokenType.Ao, c.ToString(), position));
                    position++;
                    continue;
                }

                // Иное => Unknown
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
                case "do": return TokenType.Do;
                case "end": return TokenType.End;
                case "until": return TokenType.Until;
                case "loop": return TokenType.Loop;
                case "and": return TokenType.And;
                case "or": return TokenType.Or;
                default:
                    return TokenType.Var;
            }
        }
    }
}
