using WordGrid.ViewModels;

namespace WordGrid
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var solverViewModel = DataContext as SolverViewModel;
            solverViewModel?.Initialize("./Resources/words.txt", 5);
        }
    }
}
