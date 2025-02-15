using Lab0.Classes;
using System;

namespace Lab0
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"Resources\DefaultAutomath.txt";
            var automaton = Automath.CreateFromFile(path);
            if (automaton == null)
                return;

            automaton.ShowInfo();
            Console.WriteLine();
            automaton.ShowTable();

            Console.Write("\nВведите входное слово: ");
            var inLine = Console.ReadLine();
            automaton.ProcessInputLine(inLine);

            if (automaton.Type == TypeAutomaton.NKA)
            {
                var nka = automaton as DnaAutomath;
                var dka = nka.ToDka();
                if (dka != null)
                {
                    dka.ShowInfo();
                    dka.ShowTable();
                }
            }

            if (automaton.Type == TypeAutomaton.ENKA)
            {
                var enka = automaton as NkaEpsilonAutomath;
                var nka = enka.ToNka();
                nka.ShowInfo();
                nka.ShowTable();
            }
        }
    }
}
