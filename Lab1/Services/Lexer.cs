using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Services
{
    public class Lexer
    {
        private readonly List<Lexeme> _lexemes = new();

        public void Analyze(string input)
        {
            int position = 0;

            while (position < input.Length)
            {
                char current = input[position];

                if (char.IsWhiteSpace(current))
                {
                    position++; // Пропуск пробелов
                    continue;
                }

                if (char.IsLetter(current)) // Идентификаторы и ключевые слова
                {
                    string identifier = ParseWhile(input, ref position, char.IsLetterOrDigit);
                    // Проверка на ключевые слова или добавление как идентификатор
                    _lexemes.Add(new Lexeme { Type = "Identifier", Value = identifier });
                }
                else if (char.IsDigit(current)) // Константы
                {
                    string constant = ParseWhile(input, ref position, char.IsDigit);
                    _lexemes.Add(new Lexeme { Type = "Constant", Value = constant });
                }
                else
                {
                    // Добавить обработку для специальных символов и других случаев
                    position++;
                }
            }
        }

        private string ParseWhile(string input, ref int position, Func<char, bool> condition)
        {
            int start = position;
            while (position < input.Length && condition(input[position]))
                position++;
            return input[start..position];
        }

        public List<Lexeme> GetLexemes() => _lexemes;
    }

}
