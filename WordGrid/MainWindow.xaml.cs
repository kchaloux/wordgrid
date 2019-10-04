using System.Windows.Input;
using WordGrid.ViewModels;

namespace WordGrid
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var solverViewModel = SolverView.DataContext as SolverViewModel;
            solverViewModel?.Initialize("./Resources/words.txt", 5);

            FocusManager.SetFocusedElement(SolverView, SolverView.SizeSlider);
        }
    }
}
