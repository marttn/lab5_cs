using lab5.ViewModel;
using System;
using System.Windows.Controls;

namespace lab5.View
{
    /// <summary>
    /// Логика взаимодействия для ProcessListView.xaml
    /// </summary>
    public partial class ProcessListView : UserControl
    {
        public ProcessListView()
        {
            InitializeComponent();
            DataContext = new ProcessListViewModel();
        }
        
    }
}
