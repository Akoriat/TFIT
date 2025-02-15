using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab0.Classes
{
    public class DkaAutomath : Automath
    {
        public DkaAutomath(string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
            : base(TypeAutomaton.DKA, states, inputs, finalStates, initState, transitions)
        {
        }

        public override bool ProcessInputLine(string word)
        {
            if (!IsInitiatedCorrectly)
            {
                Console.WriteLine("Операция 'ProcessInputLine' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
                return false;
            }
            var emergencyBreak = false;
            var currentState = InitState;
            List<string> inputsList = Inputs.ToList();

            Console.WriteLine($"\nТекущее состояние: {currentState}");

            foreach (var symbol in word)
            {
                if (Inputs.Contains(symbol.ToString()))
                {
                    Console.WriteLine($"Считан символ '{symbol}'");
                    var prevState = currentState;
                    currentState = Transitions[currentState][inputsList.IndexOf(symbol.ToString())];
                    if (currentState == "~")
                    {
                        Console.WriteLine($"Запрашиваемое входным символом состояние не определено.\nИз состояния {prevState} нет перехода по символу {symbol}");
                        Console.ResetColor();
                        emergencyBreak = true;
                        break;
                    }
                    Console.WriteLine($" - Текущее состояние теперь {currentState}");
                }
                else
                {
                    Console.WriteLine($"Ошибка! Считанный символ '{symbol}' не входит в алфавит!");
                    Console.ResetColor();
                    emergencyBreak = true;
                    break;
                }
            }

            if (!emergencyBreak)
            {
                Console.WriteLine($"\nВходное слово успешно прочитано. Автомат пришёл в состояние {currentState}");
                if (FinalStates.Contains(currentState))
                {
                    Console.WriteLine($"Состояние {currentState} входит в число финальных состояний.");
                    Console.ResetColor();
                    return true;
                }
                else
                {
                    Console.WriteLine($"Состояние {currentState} не входит в число финальных состояний.");
                    Console.ResetColor();
                }
            }
            return false;
        }
    }
}
