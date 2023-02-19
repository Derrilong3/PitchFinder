using System.Windows;
using System.Windows.Controls;

namespace PitchFinder.Controls
{
    /// <summary>
    /// Логика взаимодействия для ListBoxDialog.xaml
    /// </summary>
    public partial class ListBoxDialog : Window
    {
        public ListBoxDialog(string header, string[] names)
        {
            InitializeComponent();
            listBox.ItemsSource = names;
            this.header.Text = header;
        }

        public int SelectedIndex
        {
            get { return listBox.SelectedIndex; }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
