using System;
using System.Collections.Generic;
using System.Text;
using Lab1.Models;  // Для Token, TokenType, State

namespace Lab1.Services
{
    /// <summary>
    /// Лексический анализатор, использующий ДКА (детерминированный конечный автомат).
    /// </summary>
    public class Lexer
    {
        private readonly IdentifierTable _idTable;
        private readonly ConstantTable _constTable;

        // Текущее состояние автомата
        private State _state;

        // Исходный текст, над которым идёт анализ
        private string _text = string.Empty;

        // Текущая позиция символа
        private int _pos;

        // Начало текущей лексемы
        private int _lexStart;

        // Результирующий список лексем
        private readonly List<Token> _tokens = new List<Token>();

        // Конструктор
        public Lexer(IdentifierTable idTable, ConstantTable constTable)
        {
            _idTable = idTable;
            _constTable = constTable;
        }

        /// <summary>
        /// Запуск лексического анализа.
        /// </summary>
        public List<Token> Analyze(string text)
        {
            // Инициализируем поля
            _text = text;
            _pos = 0;
            _state = State.S;   // Начальное состояние
            _tokens.Clear();

            while (_state != State.E && _state != State.F)
            {
                // Если мы вышли за пределы текста, переходим в финальное состояние
                if (_pos >= _text.Length)
                {
                    _state = State.F;
                    // Если нужно, можно вызвать AddToken для последней лексемы
                    break;
                }

                char c = _text[_pos];
                bool addLex = false;  // признак, что нужно зафиксировать лексему

                State prevState = _state;

                switch (_state)
                {
                    case State.S: // Начальное состояние
                        {
                            // Пропуски
                            if (char.IsWhiteSpace(c))
                            {
                                // Остаёмся в том же состоянии S, просто двигаемся дальше
                                _pos++;
                                continue;
                            }

                            // Идентификатор/ключевое слово?
                            if (char.IsLetter(c))
                            {
                                _state = State.Ai;
                                // Запоминаем, с какой позиции началась лексема
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // Число (константа)
                            if (char.IsDigit(c))
                            {
                                _state = State.Ac;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // '<'
                            if (c == '<')
                            {
                                _state = State.As;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // '='
                            if (c == '=')
                            {
                                _state = State.Bs;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // '>' (если хотим обрабатывать аналогично '<')
                            if (c == '>')
                            {
                                _state = State.Xs; // Допустим, завели новое состояние для '>'
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // '+' или '-'
                            if (c == '+' || c == '-')
                            {
                                _state = State.Cs;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // '*' или '/'
                            if (c == '*' || c == '/')
                            {
                                _state = State.Ds;
                                _lexStart = _pos;
                                _pos++;
                                continue;
                            }

                            // Если дошли сюда — неизвестный символ
                            _state = State.E;
                            // Можно сразу добавить Unknown-лексему
                            _lexStart = _pos;
                            addLex = true;
                            break;
                        }

                    case State.Ai:
                        {
                            // Идём по буквам/цифрам (идентификатор)
                            if (char.IsLetterOrDigit(c))
                            {
                                // Не меняем состояние, просто двигаемся вперёд
                                _pos++;
                            }
                            else
                            {
                                // Граница лексемы
                                // 1) Добавляем токен (уже не читаем текущий c)
                                addLex = true;
                                // 2) Переходим в S, чтобы обработать текущий символ заново
                                _state = State.S;
                            }
                            break;
                        }

                    case State.Ac:
                        {
                            // Идём по цифрам (константа)
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
                            // Уже прочитали '<'. Проверяем, не идёт ли '=' или '>'
                            // чтобы сформировать "<=", "<>"
                            if (c == '=' || c == '>')
                            {
                                // Прочитали "<=" или "<>"
                                _pos++;
                            }
                            // Фиксируем лексему (Rel) и уходим в S
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Bs:
                        {
                            // Уже прочитали '='. Может быть "==", если нужно
                            if (c == '=')
                            {
                                // Прочитали "=="
                                _pos++;
                                // Будет Rel (==) или что-то ещё, в зависимости от языка
                            }
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Xs:
                        {
                            // Аналог для '>'
                            // Если хотим ">=", тут проверяем:
                            if (c == '=')
                            {
                                // ">"
                                _pos++;
                                // Получили ">="
                            }
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Cs:
                        {
                            // Уже прочитали '+' или '-'
                            // Считаем это отдельной лексемой, никаких вторых символов не ждём
                            addLex = true;
                            _state = State.S;
                            break;
                        }

                    case State.Ds:
                        {
                            // Уже прочитали '*' или '/'
                            // Тоже отдельная лексема
                            addLex = true;
                            _state = State.S;
                            break;
                        }
                }

                // Если выставили addLex — значит, нужно добавить лексему по предыдущему состоянию
                if (addLex)
                {
                    AddToken(prevState);
                }

                // Если мы сейчас в S или E/F, то текущий символ ещё не прочитан
                // (кроме случаев, когда мы явно increment сделали выше).
                // Но чаще всего, если мы в S, мы продолжим со следующим символом
                // в начале while.

                // Если состояние не E/F, но мы не сдвигали _pos для текущего символа:
                // — в конце цикла while мы можем сделать continue, чтобы заново обойти.
            }

            // Если вышли из цикла и состояние == F, можно добавить лексему, 
            // если она формировалась (например, в Ai или Ac).
            if (_state == State.F && _pos <= _text.Length)
            {
                // Можно проверить, не "зависла" ли последняя лексема
                // Но в примере выше мы добавляли её при addLex. 
                // Если вдруг требуется, можно вызвать AddToken(_state) 
                // (в зависимости от логики).
            }

            return _tokens;
        }

        /// <summary>
        /// Добавляет лексему (исходя из <paramref name="prevState"/>).
        /// Берёт подстроку [ _lexStart, _pos ), проверяет тип.
        /// </summary>
        private void AddToken(State prevState)
        {
            if (_pos > _text.Length) return; // На всякий случай

            // Подстрока лексемы
            int length = _pos - _lexStart;
            if (length <= 0) return;

            string lexeme = _text.Substring(_lexStart, length);

            // Определяем тип лексемы в зависимости от состояния
            TokenType ttype = TokenType.Unknown;
            int index = -1;

            switch (prevState)
            {
                case State.Ai:
                    // Идентификатор или ключевое слово
                    ttype = CheckKeyword(lexeme);
                    if (ttype == TokenType.Identifier)
                    {
                        index = _idTable.AddIdentifier(lexeme);
                    }
                    break;

                case State.Ac:
                    // Константа
                    ttype = TokenType.Constant;
                    index = _constTable.AddConstant(lexeme);
                    break;

                case State.As:
                    {
                        // Это могло быть "<", "<=", "<>"
                        // Всё — это операции сравнения
                        ttype = TokenType.Rel;
                        break;
                    }

                case State.Bs:
                    {
                        // '=' или '=='
                        if (lexeme == "=")
                        {
                            // Может быть присваивание
                            ttype = TokenType.As;
                        }
                        else if (lexeme == "==")
                        {
                            // операция сравнения
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
                        // '>' или '>='
                        // Тоже операции сравнения
                        ttype = TokenType.Rel;
                        break;
                    }

                case State.Cs:
                    {
                        // '+' или '-'
                        ttype = TokenType.Ao;
                        break;
                    }

                case State.Ds:
                    {
                        // '*' или '/'
                        ttype = TokenType.AoMulDiv;
                        break;
                    }

                case State.E:
                    {
                        // Ошибка или неизвестный символ
                        ttype = TokenType.Unknown;
                        break;
                    }
            }

            // Создаём Token
            var token = new Token(ttype, lexeme, _lexStart, index);
            _tokens.Add(token);
        }

        /// <summary>
        /// Проверка, не является ли лексема ключевым словом.
        /// Если не ключевое слово — Identifier.
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
                default: return TokenType.Identifier;
            }
        }
    }
}
