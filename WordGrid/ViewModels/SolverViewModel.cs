using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using WordGrid.Core;

namespace WordGrid.ViewModels
{
    public class SolverViewModel : BindableBase
    {
        public BoardViewModel Board
        {
            get => _board;
            private set => SetProperty(ref _board, value);
        }
        private BoardViewModel _board;

        public int Size
        {
            get => _board?.Size ?? 0;
            set
            {
                if (_board != null && _board.Size != value)
                {
                    Initialize(Words, value);
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public RelayCommand SolveBoardCommand { get; }

        public RelayCommand ClearBoardCommand { get; }

        private const string Words = "./Resources/words.txt";
        private readonly Solver _solver;

        public SolverViewModel()
        {
            _solver = new Solver(0);
            Board = new BoardViewModel();

            SolveBoardCommand = new RelayCommand(OnSolveBoardExecuted);
            ClearBoardCommand = new RelayCommand(OnClearBoardExecuted);
        }

        public void Initialize(string file, int size)
        {
            _solver.Size = size;
            _solver.Load(file);
            Board.Load(new Board(size));
        }

        private void OnSolveBoardExecuted()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var solution = _solver.FindSolutions(Board.Board).FirstOrDefault();
                if (solution != null)
                {
                    Board.Load(solution);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OnClearBoardExecuted()
        {
            Board.Clear();
        }
    }
}
