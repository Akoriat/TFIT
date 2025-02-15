using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab0.Classes
{
    public class DnaAutomath : Automath
    {
        public DnaAutomath(string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
            : base(TypeAutomaton.NKA, states, inputs, finalStates, initState, transitions)
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
            List<string> inputsList = Inputs.ToList();
            List<string> reachableStates = new List<string>();
            List<string> currentStates = new List<string>() { InitState };

            Console.WriteLine($"\nТекущее состояние: {InitState}");

            foreach (var symbol in word)
            {
                if (Inputs.Contains(symbol.ToString()))
                {
                    Console.WriteLine($"Считан символ '{symbol}'");
                    foreach (var item in currentStates)
                    {
                        var tempState = Transitions[item][inputsList.IndexOf(symbol.ToString())];
                        if (tempState.Contains("{"))
                        {
                            tempState = tempState.Trim('{', '}');
                            foreach (var state in tempState.Split(','))
                            {
                                if (!reachableStates.Contains(state) && state != "~")
                                    reachableStates.Add(state);
                            }
                        }
                        else
                        {
                            if (tempState == "~" && currentStates.Count == 1)
                            {
                                Console.WriteLine($"Запрашиваемое входным символом состояние не определено.\nИз состояния {currentStates[0]} нет перехода по символу {symbol}");
                                Console.ResetColor();
                                emergencyBreak = true;
                                return false;
                            }
                            if (!reachableStates.Contains(tempState) && tempState != "~")
                                reachableStates.Add(tempState);
                        }
                    }

                    if (reachableStates.Count == 0)
                    {
                        Console.WriteLine($"Запрашиваемое входным символом состояние не определено.\nИз состояний нет перехода по символу {symbol}");
                        Console.ResetColor();
                        emergencyBreak = true;
                        return false;
                    }

                    currentStates = new List<string>(reachableStates);
                    reachableStates.Clear();
                    Console.Write(" - Текущее(ие) состояние(ия): ");
                    foreach (var item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine();
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
                if (currentStates.Any(state => FinalStates.Contains(state)))
                {
                    Console.Write("Одно из состояний ");
                    foreach (var item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("входит в число финальных состояний.");
                    Console.ResetColor();
                    return true;
                }
                else
                {
                    Console.Write("Ни одно из состояний ");
                    foreach (var item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("не входит в число финальных состояний.");
                    Console.ResetColor();
                }
            }
            return false;
        }

        public DkaAutomath ToDka()
        {
            Console.WriteLine("\nВыполняется преобразование НКА к ДКА:\n");
            Console.ResetColor();

            List<string> newStates = new List<string>();
            List<string> newFinalStates = new List<string>();
            Dictionary<string, List<string>> newTransitions = new Dictionary<string, List<string>>();
            Queue<string> queue = new Queue<string>();

            var initialState = "{" + InitState + "}";
            queue.Enqueue(initialState);
            newStates.Add(initialState);

            if (FinalStates.Contains(InitState))
            {
                newFinalStates.Add(initialState);
            }

            while (queue.Count > 0)
            {
                var currentState = queue.Dequeue();
                List<string> results = new List<string>();
                HashSet<string>[] reachableFromInputs = new HashSet<string>[Inputs.Length];

                for (var j = 0; j < Inputs.Length; j++)
                {
                    reachableFromInputs[j] = new HashSet<string>();
                }

                string[] componentStates = currentState.Trim('{', '}').Split(',');

                foreach (var state in componentStates)
                {
                    for (var j = 0; j < Inputs.Length; j++)
                    {
                        if (Transitions.ContainsKey(state))
                        {
                            string[] nextStates = Transitions[state][j].Trim('{', '}').Split(',');
                            foreach (var nextState in nextStates)
                            {
                                if (nextState != "~")
                                {
                                    reachableFromInputs[j].Add(nextState);
                                }
                            }
                        }
                    }
                }

                for (var j = 0; j < Inputs.Length; j++)
                {
                    if (reachableFromInputs[j].Count > 0)
                    {
                        var newState = "{" + string.Join(",", reachableFromInputs[j].OrderBy(s => s)) + "}";
                        results.Add(newState);

                        if (!newStates.Contains(newState))
                        {
                            queue.Enqueue(newState);
                            newStates.Add(newState);

                            if (reachableFromInputs[j].Any(s => FinalStates.Contains(s)))
                            {
                                newFinalStates.Add(newState);
                            }
                        }
                    }
                    else
                    {
                        results.Add("~");
                    }
                }

                newTransitions[currentState] = results;
            }

            newStates.Remove("~");
            return new DkaAutomath(newStates.ToArray(), Inputs, newFinalStates.Distinct().ToArray(), initialState, newTransitions);
        }
    }
}
