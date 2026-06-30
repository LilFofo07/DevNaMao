using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PDVModerno.Views
{
    public partial class AdicionarMultiploDialog : Window
    {
        public string CodigoBarrasLido { get; private set; } = string.Empty;
        public int Quantidade { get; private set; } = 1;

        public AdicionarMultiploDialog()
        {
            InitializeComponent();
            TxtQuantidade.PreviewTextInput += TxtQuantidade_PreviewTextInput;
            
            // Foca no campo de quantidade primeiro para agilizar
            Loaded += (s, e) => { TxtQuantidade.Focus(); TxtQuantidade.SelectAll(); };
        }

        private void TxtQuantidade_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TxtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Confirmar();
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            Confirmar();
        }

        private void Confirmar()
        {
            if (string.IsNullOrWhiteSpace(TxtCodigoBarras.Text))
            {
                MessageBox.Show("Por favor, informe ou bipe o código de barras.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCodigoBarras.Focus();
                return;
            }

            if (int.TryParse(TxtQuantidade.Text, out int qtd) && qtd > 0)
            {
                Quantidade = qtd;
                CodigoBarrasLido = TxtCodigoBarras.Text.Trim();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Quantidade inválida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtQuantidade.Focus();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
