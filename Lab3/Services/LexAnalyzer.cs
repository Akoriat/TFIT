using System;
using System.Collections.Generic;
using Lab3.Models;

namespace Lab3.Services
{
    public class LexAnalyzer
    {
        public List<Token> Tokens { get; private set; } = new List<Token>();
        public List<string> Identifiers { get; private set; } = new List<string>();
        public List<string> Constants { get; private set; } = new List<string>();

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

        public bool Analyze(string input)
        {
            var pos = 0;
            while (pos < input.Length)
            {
                var c = input[pos];

                if (char.IsWhiteSpace(c))
                {
                    pos++;
                    continue;
                }

                if (char.IsLetter(c))
                {
                    var start = pos;
                    while (pos < input.Length && char.IsLetterOrDigit(input[pos]))
                    {
                        pos++;
                    }
                    var lexeme = input.Substring(start, pos - start);
                    var lowerLexeme = lexeme.ToLower();
                    if (keywords.ContainsKey(lowerLexeme))
                    {
                        var token = new Token { Type = keywords[lowerLexeme], Lexeme = lexeme, StartPos = start, EndPos = pos - 1 };
                        Tokens.Add(token);
                    }
                    else
                    {
                        var token = new Token { Type = TokenType.Identifier, Lexeme = lexeme, StartPos = start, EndPos = pos - 1 };
                        Tokens.Add(token);
                        if (!Identifiers.Contains(lexeme))
                            Identifiers.Add(lexeme);
                    }
                    continue;
                }
                else if (char.IsDigit(c))
                {
                    var start = pos;
                    while (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        pos++;
                    }
                    var lexeme = input.Substring(start, pos - start);
                    var token = new Token { Type = TokenType.Constant, Lexeme = lexeme, StartPos = start, EndPos = pos - 1 };
                    Tokens.Add(token);
                    if (!Constants.Contains(lexeme))
                        Constants.Add(lexeme);
                    continue;
                }
                else
                {
                    if (c == '<')
                    {
                        if (pos + 1 < input.Length && input[pos + 1] == '>')
                        {
                            var token = new Token { Type = TokenType.Rel, Lexeme = "<>", StartPos = pos, EndPos = pos + 1 };
                            Tokens.Add(token);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            var token = new Token { Type = TokenType.Rel, Lexeme = "<", StartPos = pos, EndPos = pos };
                            Tokens.Add(token);
                            pos++;
                            continue;
                        }
                    }
                    else if (c == '>')
                    {
                        var token = new Token { Type = TokenType.Rel, Lexeme = ">", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '=')
                    {
                        if (pos + 1 < input.Length && input[pos + 1] == '=')
                        {
                            var token = new Token { Type = TokenType.Rel, Lexeme = "==", StartPos = pos, EndPos = pos + 1 };
                            Tokens.Add(token);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            var token = new Token { Type = TokenType.As, Lexeme = "=", StartPos = pos, EndPos = pos };
                            Tokens.Add(token);
                            pos++;
                            continue;
                        }
                    }
                    else if (c == '+')
                    {
                        var token = new Token { Type = TokenType.Plus, Lexeme = "+", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '-')
                    {
                        var token = new Token { Type = TokenType.Minus, Lexeme = "-", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '*')
                    {
                        var token = new Token { Type = TokenType.Multiply, Lexeme = "*", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == '/')
                    {
                        var token = new Token { Type = TokenType.Divide, Lexeme = "/", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else if (c == ';')
                    {
                        var token = new Token { Type = TokenType.Delimiter, Lexeme = ";", StartPos = pos, EndPos = pos };
                        Tokens.Add(token);
                        pos++;
                        continue;
                    }
                    else
                    {
                        var token = new Token { Type = TokenType.Unknown, Lexeme = c.ToString(), StartPos = pos, EndPos = pos };
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
