using Lab4.Models;

namespace Lab4.Services
{
    public class Lexer
    {
        private Token? _pFirst = null;
        private Token? _pLast = null;

        private static LexemeCategory GetCategory(TokenType type)
        {
            return type switch
            {
                TokenType.Not or TokenType.And or TokenType.Or or TokenType.Output or
                TokenType.Do or TokenType.Until or TokenType.Loop => LexemeCategory.Keyword,
                TokenType.Constant => LexemeCategory.Constant,
                TokenType.Identifier => LexemeCategory.Identifier,
                _ => LexemeCategory.SpecialSymbol,
            };
        }

        private void AddToken(TokenType type, string lexeme, int pos)
        {
            Token token = new()
            {
                Type = type,
                Lexeme = lexeme,
                Position = pos,
                Category = GetCategory(type),
                Next = null
            };
            if (_pFirst == null)
            {
                _pFirst = token;
                _pLast = token;
            }
            else
            {
                if (_pLast != null)
                    _pLast.Next = token;
                _pLast = token;
            }
        }

        public Token? LexAnalysis(string text)
        {
            int pos = 0;
            int length = text.Length;

            while (pos < length)
            {
                char currentChar = text[pos];

                if (char.IsWhiteSpace(currentChar))
                {
                    pos++;
                    continue;
                }

                if (char.IsLetter(currentChar))
                {
                    int start = pos;
                    while (pos < length && char.IsLetterOrDigit(text[pos]))
                        pos++;
                    string lexeme = text[start..pos];
                    TokenType type = lexeme.ToLower() switch
                    {
                        "not" => TokenType.Not,
                        "and" => TokenType.And,
                        "or" => TokenType.Or,
                        "output" => TokenType.Output,
                        "do" => TokenType.Do,
                        "until" => TokenType.Until,
                        "loop" => TokenType.Loop,
                        _ => TokenType.Identifier,
                    };
                    AddToken(type, lexeme, start);
                    continue;
                }
                else if (char.IsDigit(currentChar))
                {
                    int start = pos;
                    while (pos < length && char.IsDigit(text[pos]))
                        pos++;
                    string lexeme = text[start..pos];
                    AddToken(TokenType.Constant, lexeme, start);
                    continue;
                }
                else
                {
                    switch (currentChar)
                    {
                        case '<':
                            if (pos + 1 < length && text[pos + 1] == '>')
                            {
                                AddToken(TokenType.Rel, "<>", pos);
                                pos += 2;
                            }
                            else
                            {
                                AddToken(TokenType.Rel, "<", pos);
                                pos++;
                            }
                            break;
                        case '=':
                            if (pos + 1 < length && text[pos + 1] == '=')
                            {
                                AddToken(TokenType.Rel, "==", pos);
                                pos += 2;
                            }
                            else
                            {
                                AddToken(TokenType.As, "=", pos);
                                pos++;
                            }
                            break;
                        case '>':
                            AddToken(TokenType.Rel, ">", pos);
                            pos++;
                            break;
                        case '+':
                            AddToken(TokenType.Ao, "+", pos);
                            pos++;
                            break;
                        case '-':
                            AddToken(TokenType.Ao, "-", pos);
                            pos++;
                            break;
                        case '*':
                            AddToken(TokenType.Mul, "*", pos);
                            pos++;
                            break;
                        case '/':
                            AddToken(TokenType.Div, "/", pos);
                            pos++;
                            break;
                        case ';':
                            AddToken(TokenType.Del, ";", pos);
                            pos++;
                            break;
                        default:
                            AddToken(TokenType.Identifier, currentChar.ToString(), pos);
                            pos++;
                            break;
                    }
                }
            }

            AddToken(TokenType.EndOfFile, string.Empty, pos);
            return _pFirst;
        }
    }
}
