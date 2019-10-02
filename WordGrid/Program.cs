using System;
using System.Diagnostics;

namespace WordGrid
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new Solver(5);
            solver.Load(@"Resources/words.txt");
            var board = new Board(solver.Size);
            Console.ReadLine();
        }
    }
}
