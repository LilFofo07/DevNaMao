using System.Collections.ObjectModel;
using System.Windows;
using PDVModerno.Models;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PDVModerno.Views
{
    public partial class DescontoDialog : Window
    {
        public ItemVenda ItemSelecionado { get; private set; }
        public decimal DescontoAplicado { get; private set; }

        public DescontoDialog(ObservableCollection<ItemVenda> carrinho)
        {
            InitializeComponent();
            
            CmbProdutos.ItemsSource = carrinho;
            if (carrinho.Count > 0)
            {
                CmbProdutos.SelectedIndex = 0;
            }
            
            // Permite apenas números e vírgula
            TxtDesconto.PreviewTextInput += TxtDesconto_PreviewTextInput;
        }

        private void TxtDesconto_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProdutos.SelectedItem is ItemVenda item)
            {
                if (decimal.TryParse(TxtDesconto.Text, out decimal desconto))
                {
                    ItemSelecionado = item;
                    DescontoAplicado = desconto;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Valor de desconto inválido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Selecione um produto.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
