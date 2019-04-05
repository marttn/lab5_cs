using lab5.View;
using System.Windows;

namespace lab5
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProcessListView processListView;
        public MainWindow()
        {
            InitializeComponent();
            Main.Children.Clear();
            if (processListView == null)
                processListView = new ProcessListView();
            Main.Children.Add(processListView);
        }
        
    }
}
