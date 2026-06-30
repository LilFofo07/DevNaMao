using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PDVModerno.Views
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            LblQuestion.Text = question;
            TxtAnswer.Text = defaultAnswer;
            TxtAnswer.Focus();
            TxtAnswer.SelectAll();
            
            // Permite apenas números na caixa de texto
            TxtAnswer.PreviewTextInput += TxtAnswer_PreviewTextInput;
        }

        private void TxtAnswer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            Answer = TxtAnswer.Text;
            DialogResult = true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
