using System.Windows;

namespace PitchFinder.Controls
{
    /// <summary>
    /// Логика взаимодействия для InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string header)
        {
            InitializeComponent();
            this.header.Text = header;
        }

        public string InputText
        {
            get { return inputText.Text; }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
