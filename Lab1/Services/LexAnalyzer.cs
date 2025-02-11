using System;
using System.Collections.Generic;
using Lab1.Models;

namespace Lab1.Services
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
            int pos = 0;
            while (pos < input.Length)
            {
                char c = input[pos];

                if (char.IsWhiteSpace(c))
                {
                    pos++;
                    continue;
                }

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
                        Token token = new Token { Type = keywords[lowerLexeme], Lexeme = lexeme, Position = start };
                        Tokens.Add(token);
                    }
                    else
                    {
                        Token token = new Token { Type = TokenType.Identifier, Lexeme = lexeme, Position = start };
                        Tokens.Add(token);
                        if (!Identifiers.Contains(lexeme))
                            Identifiers.Add(lexeme);
                    }
                    continue;
                }
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
                else
                {
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
                    else if (c == '=')
                    {
                        if (pos + 1 < input.Length && input[pos + 1] == '=')
                        {
                            Token token = new Token { Type = TokenType.Equal, Lexeme = "==", Position = pos };
                            Tokens.Add(token);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            Token token = new Token { Type = TokenType.Assigment, Lexeme = "=", Position = pos };
                            Tokens.Add(token);
                            pos++;
                            continue;
                        }
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
