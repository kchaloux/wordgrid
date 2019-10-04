using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        public IList SelectedCells => _selectedCells;
        private readonly ObservableCollection<object> _selectedCells;

        public RelayCommand<KeyEventArgs> PreviewKeyDownCommand { get; }

        public BoardViewModel()
        {
            _cells = new ObservableCollection<BoardCellViewModel>();
            _selectedCells = new ObservableCollection<object>();
            _selectedCells.CollectionChanged += OnSelectedItemsChanged;
            PreviewKeyDownCommand = new RelayCommand<KeyEventArgs>(OnPreviewKeyDownExecuted);
        }

        public void Load(Board board)
        {
            Board = board;
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
            _selectedCells.CollectionChanged -= OnSelectedItemsChanged;
            _selectedCells.Clear();
            for (var i = 0; i < Size; ++i)
            {
                for (var j = 0; j < Size; ++j)
                {
                    var index = i * Size + j;
                    _cells[index].C = " ";
                    _cells[index].IsSelected = false;
                    Board[i, j] = ' ';
                }
            }
            _selectedCells.CollectionChanged += OnSelectedItemsChanged;
        }

        private void OnPreviewKeyDownExecuted(KeyEventArgs e)
        {
            var selectedIndices = new List<int>();
            for (var i = 0; i < _cells.Count; ++i)
            {
                if (_cells[i].IsSelected)
                {
                    selectedIndices.Add(i);
                }
            }

            if (!selectedIndices.Any())
            {
                return;
            }

            int selectedIndex = -1;
            switch (e.Key)
            {
                case Key.Space:
                case Key.Delete:
                case Key.Back:
                case Key key when key >= Key.A && key <= Key.Z:
                    var letter = e.Key >= Key.A && e.Key <= Key.Z
                        ? ((char)(e.Key - Key.A + 'A')).ToString()
                        : " ";
                    foreach (var index in selectedIndices)
                    {
                        var (row, col) = RowAndColumn(index);
                        Board[row, col] = char.ToLowerInvariant(letter[0]);
                        _cells[index].C = letter;
                    }
                    break;
                case Key.Left:
                    selectedIndex = OffsetIndex(selectedIndices[0], 0, -1);
                    break;
                case Key.Up:
                    selectedIndex = OffsetIndex(selectedIndices[0], -1, 0);
                    break;
                case Key.Tab:
                case Key.Right:
                    selectedIndex = OffsetIndex(selectedIndices[0], 0, 1);
                    break;
                case Key.Enter:
                case Key.Down:
                    selectedIndex = OffsetIndex(selectedIndices[0], 1, 0);
                    break;
                default:
                    return;
            }

            if (selectedIndex >= 0)
            {
                _selectedCells.CollectionChanged -= OnSelectedItemsChanged;
                _selectedCells.Clear();
                for (var i = 0; i < _cells.Count; ++i)
                {
                    if (i == selectedIndex)
                    {
                        _cells[i].IsSelected = true;
                        _selectedCells.Add(_cells[i]);
                    }
                    else
                    {
                        _cells[i].IsSelected = false;
                    }
                }
                _selectedCells.CollectionChanged += OnSelectedItemsChanged;
            }
            e.Handled = true;
        }

        private (int row, int col) RowAndColumn(int index)
        {
            var col = index % Size;
            var row = (index - col) / Size;
            return (row, col);
        }

        private int OffsetIndex(int index, int rowOffset, int colOffset)
        {
            var (row, col) = RowAndColumn(index);
            row = (row + rowOffset + Size) % Size;
            col = (col + colOffset + Size) % Size;
            return row * Size + col;
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var unselected = e.OldItems?.OfType<BoardCellViewModel>().ToList() ?? new List<BoardCellViewModel>();
            foreach (var item in unselected)
            {
                item.IsSelected = false;
            }

            var selected = e.NewItems?.OfType<BoardCellViewModel>().ToList() ?? new List<BoardCellViewModel>();
            foreach (var item in selected)
            {
                item.IsSelected = true;
            }
        }
    }
}
