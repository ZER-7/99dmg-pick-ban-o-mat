namespace PickBan_o_mat
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Viewmodel vm = new Viewmodel();
            DataContext = vm;
        }
    }
}