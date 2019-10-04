using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using WordGrid.Core;

namespace WordGrid.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BoardViewModel : BindableBase
    {
        public Board Board
        {
            get => _board;
            set
            {
                if (SetProperty(ref _board, value))
                {
                    OnPropertyChanged(nameof(Size));
                }
            }
        }
        private Board _board;

        public int Size => Board?.Size ?? 0;

        public IReadOnlyList<BoardCellViewModel> Cells => _cells;
        private readonly ObservableCollection<BoardCellViewModel> _cells;

        public int SelectedCellIndex
        {
            get => _selectedCellIndex;
            set
            {
                var oldSelectedCellIndex = _selectedCellIndex;
                if (SetProperty(ref _selectedCellIndex, value))
                {
                    if (oldSelectedCellIndex >= 0 && oldSelectedCellIndex < _cells.Count)
                    {
                        _cells[oldSelectedCellIndex].IsSelected = false;
                    }
                    if (_selectedCellIndex >= 0 && _selectedCellIndex < _cells.Count)
                    {
                        _cells[_selectedCellIndex].IsSelected = true;
                    }
                }
            }
        }
        private int _selectedCellIndex = -1;

        public RelayCommand<KeyEventArgs> PreviewKeyDownCommand { get; }

        public BoardViewModel()
        {
            _cells = new ObservableCollection<BoardCellViewModel>();
            PreviewKeyDownCommand = new RelayCommand<KeyEventArgs>(OnPreviewKeyDownExecuted);
        }

        public void Load(Board board)
        {
            Board = board;
            SelectedCellIndex = -1;
            _cells.Clear();

            for (var i = 0; i < board.Size; ++i)
            {
                for (var j = 0; j < board.Size; ++j)
                {
                    var cell = new BoardCellViewModel { C = board[i, j].ToString().ToUpperInvariant() };
                    _cells.Add(cell);
                }
            }
        }

        public void Clear()
        {
            for (var i = 0; i < Size; ++i)
            {
                for (var j = 0; j < Size; ++j)
                {
                    _cells[(i * Size) + j].C = " ";
                    Board[i, j] = ' ';
                }
            }
        }

        private void OnPreviewKeyDownExecuted(KeyEventArgs e)
        {
            if (_selectedCellIndex < 0 || _selectedCellIndex >= _cells.Count)
            {
                return;
            }

            string letter = null;
            var col = _selectedCellIndex % Size;
            var row = (_selectedCellIndex - col) / Size;
            switch (e.Key)
            {
                case Key.Space:
                case Key.Delete:
                case Key.Back:
                    letter = " ";
                    break;
                case Key key when key >= Key.A && key <= Key.Z:
                    letter = ((char)(e.Key - Key.A + 'A')).ToString();
                    break;
                case Key.Up:
                    row = (row - 1 + Size) % Size;
                    break;
                case Key.Enter:
                case Key.Down:
                    row = (row + 1) % Size;
                    break;
                case Key.Left:
                    col = (col - 1 + Size) % Size;
                    break;
                case Key.Tab:
                case Key.Right:
                    col = (col + 1) % Size;
                    break;
                default:
                    return;
            }

            if (letter != null)
            {
                _cells[_selectedCellIndex].C = letter;
                Board[row, col] = char.ToLowerInvariant(letter[0]);
            }
            SelectedCellIndex = row * Size + col;

            e.Handled = true;
        }
    }
}
