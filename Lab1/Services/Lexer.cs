using System;
using System.Collections.Generic;
using System.Text;
using Lab1.Models;

namespace Lab1.Services
{
    public class Lexer
    {
        private readonly IdentifierTable _idTable;
        private readonly ConstantTable _constTable;

        //Текущее состояние автомата
        private State _state;

        //Исходный текст
        private string _text = string.Empty;

        //позиция символа
        private int _pos;

        //Начало текущей лексемы
        private int _lexStart;

        //список лексем
        private readonly List<Token> _tokens = new List<Token>();

        public Lexer(IdentifierTable idTable, ConstantTable constTable)
        {
            _idTable = idTable;
            _constTable = constTable;
        }

        public List<Token> Analyze(string text)
        {
            _text = text;
            _pos = 0;
            _state = State.S;
            _tokens.Clear();

            while (_state != State.E && _state != State.F)
            {
                if (_pos >= _text.Length)
                {
                    _state = State.F;
                    break;
                }

                char c = _text[_pos];
                bool addLex = false;  // признак, что нужно зафиксировать лексему

                State prevState = _state;

                switch (_state)
                {
                    case State.S:
                        {
                            if (char.IsWhiteSpace(c))
                            {
                                _pos++;
                                continue;
                            }

                            if (char.IsLetter(c))
                            {
                                _state = State.Ai;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (char.IsDigit(c))
                            {
                                _state = State.Ac;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (c == '<')
                            {
                                _state = State.As;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (c == '=')
                            {
                                _state = State.Bs;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (c == '>')
                            {
                                _state = State.Xs;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (c == '+' || c == '-')
                            {
                                _state = State.Cs;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            if (c == '*' || c == '/')
                            {
                                _state = State.Ds;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            _state = State.E;
                            _lexStart = _pos;
                            addLex = true;
                            break;
                        }

                    case State.Ai:
                        {
                            if (char.IsLetterOrDigit(c))
                            {
                                _pos++;
                            }
                            else
                            {
                                addLex = true;
                                _state = State.S;
                            }
                            break;
                        }

                    case State.Ac:
                        {
                            if (char.IsDigit(c))
                            {
                                _pos++;
                            }
                            else
                            {
                                addLex = true;
                                _state = State.S;
                            }
                            break;
                        }

                    case State.As:
                        {
                            if (c == '=' || c == '>')
                            {
                                _pos++;
                            }
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Bs:
                        {
                            if (c == '=')
                            {
                                _pos++;
                            }
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Xs:
                        {
                            if (c == '=')
                            {
                                _pos++;
                            }
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Cs:
                        {
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Ds:
                        {
                            addLex = true;
                            _state = State.S;
                            break;
                        }
                }

                if (addLex)
                {
                    AddToken(prevState);
                }
            }
            return _tokens;
        }

        private void AddToken(State prevState)
        {
            if (_pos > _text.Length) return;

            int length = _pos - _lexStart;
            if (length <= 0) return;

            string lexeme = _text.Substring(_lexStart, length);

            TokenType ttype = TokenType.Unknown;
            int index = -1;

            switch (prevState)
            {
                case State.Ai:
                    ttype = CheckKeyword(lexeme);
                    if (ttype == TokenType.Identifier)
                    {
                        index = _idTable.AddIdentifier(lexeme);
                    }
                    break;

                case State.Ac:
                    ttype = TokenType.Constant;
                    index = _constTable.AddConstant(lexeme);
                    break;

                case State.As:
                    {
                        ttype = TokenType.Rel;
                        break;
                    }

                case State.Bs:
                    {
                        if (lexeme == "=")
                        {
                            ttype = TokenType.As;
                        }
                        else if (lexeme == "==")
                        {
                            ttype = TokenType.Rel;
                        }
                        else
                        {
                            ttype = TokenType.Unknown;
                        }
                        break;
                    }

                case State.Xs:
                    {
                        ttype = TokenType.Rel;
                        break;
                    }

                case State.Cs:
                    {
                        ttype = TokenType.Ao;
                        break;
                    }

                case State.Ds:
                    {
                        ttype = TokenType.AoMulDiv;
                        break;
                    }

                case State.E:
                    {
                        ttype = TokenType.Unknown;
                        break;
                    }
            }

            var token = new Token(ttype, lexeme, _lexStart, index);
            _tokens.Add(token);
        }

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
                default: return TokenType.Identifier;
            }
        }
    }
}
