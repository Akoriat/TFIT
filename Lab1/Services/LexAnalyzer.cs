using System;
using System.Collections.Generic;
using Lab1.Models;

namespace Lab1.Services
{

    // Лексический анализатор
    public class LexAnalyzer
    {
        public List<Token> Tokens { get; private set; } = new List<Token>();
        public List<string> Identifiers { get; private set; } = new List<string>();
        public List<string> Constants { get; private set; } = new List<string>();

        // Таблица ключевых слов (все в нижнем регистре)
        private Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "do", TokenType.Do },
            { "until", TokenType.Until },
            { "loop", TokenType.Loop },
            { "output", TokenType.Output },
            { "not", TokenType.Not },
            { "and", TokenType.And },
            { "or", TokenType.Or }
        };

        /// <summary>
        /// Метод, выполняющий лексический анализ входного текста.
        /// Для простоты реализован как сканирование по символам с учётом правил:
        /// - Идентификаторы: начинаются с буквы, далее буквы или цифры;
        /// - Константы: последовательности цифр;
        /// - Ключевые слова: сравнение найденного идентификатора с таблицей keywords;
        /// - Специальные символы: операторы сравнения (<, >, <>), знак равенства, арифметические операторы, разделитель (;).
        /// </summary>
        public bool Analyze(string input)
        {
            int pos = 0;
            while (pos < input.Length)
            {
                char c = input[pos];

                // Пропускаем незначащие (пробельные) символы
                if (char.IsWhiteSpace(c))
                {
                    pos++;
                    continue;
                }

                // Если символ – буква, читаем идентификатор или ключевое слово
                if (char.IsLetter(c))
                {
                    int start = pos;
                    while (pos < input.Length && char.IsLetterOrDigit(input[pos]))
                    {
                        pos++;
                    }
                    string lexeme = input.Substring(start, pos - start);
                    string lowerLexeme = lexeme.ToLower();
                    if (keywords.ContainsKey(lowerLexeme))
                    {
                        // Ключевое слово
                        Token token = new Token { Type = keywords[lowerLexeme], Lexeme = lexeme, Position = start };
                        Tokens.Add(token);
                    }
                    else
                    {
                        // Идентификатор
                        Token token = new Token { Type = TokenType.Identifier, Lexeme = lexeme, Position = start };
                        Tokens.Add(token);
                        if (!Identifiers.Contains(lexeme))
                            Identifiers.Add(lexeme);
                    }
                    continue;
                }
                // Если символ – цифра, читаем константу
                else if (char.IsDigit(c))
                {
                    int start = pos;
                    while (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        pos++;
                    }
                    string lexeme = input.Substring(start, pos - start);
                    Token token = new Token { Type = TokenType.Constant, Lexeme = lexeme, Position = start };
                    Tokens.Add(token);
                    if (!Constants.Contains(lexeme))
                        Constants.Add(lexeme);
                    continue;
                }
                // Если символ – специальный символ (операторы, разделители)
                else
                {
                    // Для оператора «<>» – два символа
                    if (c == '<')
                    {
                        if (pos + 1 < input.Length && input[pos + 1] == '>')
                        {
                            Token token = new Token { Type = TokenType.RelOp, Lexeme = "<>", Position = pos };
                            Tokens.Add(token);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            Token token = new Token { Type = TokenType.RelOp, Lexeme = "<", Position = pos };
                            Tokens.Add(token);
                            pos++;
                            continue;
                        }
                    }
                    else if (c == '>')
                    {
                        Token token = new Token { Type = TokenType.RelOp, Lexeme = ">", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    // Знак равенства – используется и в операциях сравнения, и в присваивании.
                    else if (c == '=')
                    {
                        Token token = new Token { Type = TokenType.Equal, Lexeme = "=", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '+')
                    {
                        Token token = new Token { Type = TokenType.Plus, Lexeme = "+", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '-')
                    {
                        Token token = new Token { Type = TokenType.Minus, Lexeme = "-", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '*')
                    {
                        Token token = new Token { Type = TokenType.Multiply, Lexeme = "*", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '/')
                    {
                        Token token = new Token { Type = TokenType.Divide, Lexeme = "/", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == ';')
                    {
                        Token token = new Token { Type = TokenType.Semicolon, Lexeme = ";", Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else
                    {
                        // Если символ не распознан – добавляем токен типа Unknown
                        Token token = new Token { Type = TokenType.Unknown, Lexeme = c.ToString(), Position = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                }
            }
            return true;
        }
    }
}
