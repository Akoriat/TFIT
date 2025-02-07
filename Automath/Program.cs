using Lab0.Classes;

namespace Lab0;

class Program
{
    static void Main(string[] args)
    {
        string path = @"Resources\example.txt";
        Automaton newAutomath = new Automaton(path);
        newAutomath.ShowInfo();
        Console.WriteLine();
        newAutomath.ShowTable();

        string inLine;
        Console.Write("\nВведите входное слово: ");
        inLine = Console.ReadLine();
        newAutomath.ProcessInputLine(inLine);

        if (newAutomath.type == TypeAutomaton.NKA) // для случая недетерминированного автомата
        {
            Automaton newAuto = newAutomath.knaToKda();
            if (newAuto != null)
            {
                newAuto.ShowInfo();
                newAuto.ShowTable();
            }
        }

        if (newAutomath.type == TypeAutomaton.ENKA)
        {
            Automaton newAuto = newAutomath.knaEpsToKna();
            newAuto.ShowInfo();
            newAuto.ShowTable();
        }
    }
}