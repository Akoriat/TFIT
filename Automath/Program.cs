using Lab0.Classes;
using System;

namespace Lab0
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Resources\example.txt";
            Automaton automaton = Automaton.CreateFromFile(path);
            if (automaton == null)
                return;

            automaton.ShowInfo();
            Console.WriteLine();
            automaton.ShowTable();

            Console.Write("\nВведите входное слово: ");
            string inLine = Console.ReadLine();
            automaton.ProcessInputLine(inLine);

            // Если автомат – НКА, выполняем преобразование в ДКА
            if (automaton.Type == TypeAutomaton.NKA)
            {
                NonDeterministicAutomaton nka = automaton as NonDeterministicAutomaton;
                DeterministicAutomaton dka = nka.ToDeterministic();
                if (dka != null)
                {
                    dka.ShowInfo();
                    dka.ShowTable();
                }
            }

            // Если автомат – ЕНКА, выполняем преобразование в "обычный" НКА
            if (automaton.Type == TypeAutomaton.ENKA)
            {
                EpsilonAutomaton enka = automaton as EpsilonAutomaton;
                NonDeterministicAutomaton nka = enka.ToNonDeterministic();
                nka.ShowInfo();
                nka.ShowTable();
            }
        }
    }
}
