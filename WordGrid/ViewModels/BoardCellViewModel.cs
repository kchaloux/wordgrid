namespace WordGrid.ViewModels
{
    public class BoardCellViewModel : BindableBase
    {
        public string C
        {
            get => _c;
            set => SetProperty(ref _c, value);
        }
        private string _c;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private bool _isSelected;
    }
}
