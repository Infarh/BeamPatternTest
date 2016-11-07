using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace BeamPatternWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BeamLoadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var model = button.DataContext as Model;
            if (model == null) return;

            var dialog = new OpenFileDialog();
            dialog.Title = "Выбор файла с данными ДН";
            dialog.Filter = "Текстовые данные (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() != true) return;

            var file_name = dialog.FileName;

            model.ReadPatternFromFile(file_name);
        }
    }
}
