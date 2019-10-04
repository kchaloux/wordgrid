using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        public RelayCommand<MouseButtonEventArgs> PreviewMouseButtonDownCommand { get; }

        public RelayCommand SolveBoardCommand { get; }

        public RelayCommand ClearBoardCommand { get; }

        private readonly Solver _solver;

        public SolverViewModel()
        {
            _solver = new Solver(0);
            Board = new BoardViewModel();

            PreviewMouseButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseButtonDownExecuted);
            SolveBoardCommand = new RelayCommand(OnSolveBoardExecuted);
            ClearBoardCommand = new RelayCommand(OnClearBoardExecuted);
        }

        public void Initialize(string file, int size)
        {
            _solver.Size = size;
            _solver.Load(file);
            Board.Load(new Board(size));
        }

        private void OnPreviewMouseButtonDownExecuted(MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            while (source != null)
            {
                if ((source as FrameworkElement)?.DataContext == Board)
                {
                    return;
                }
                source = VisualTreeHelper.GetParent(source);
            }
            Board.SelectedCellIndex = -1;
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
