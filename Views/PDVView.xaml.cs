using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDVModerno.Models;
using PDVModerno.Utils;

namespace PDVModerno.Views
{
    public partial class PDVView : UserControl
    {
        public ObservableCollection<ItemVenda> Carrinho { get; set; } = new ObservableCollection<ItemVenda>();
        
        private int _multiplicadorProximoItem = 1;
        private bool _isCaixaAberto = true;

        public PDVView()
        {
            InitializeComponent();
            
            // Vincular lista de produtos da tela com o AppState global
            ListaProdutos.ItemsSource = AppState.Produtos;
            
            // Vincular carrinho
            ListaCarrinho.ItemsSource = Carrinho;
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCaixaAberto) return;

            if (sender is Button btn && btn.Tag is Produto produtoSelecionado)
            {
                AdicionarProdutoAoCarrinho(produtoSelecionado);
            }
        }

        private void BtnRemover_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenda itemRemover)
            {
                if (itemRemover.Quantidade > 1)
                {
                    var dialog = new InputDialog($"Você tem {itemRemover.Quantidade}x {itemRemover.Produto.Nome}. Quantos deseja remover?", "1");
                    
                    // Mostra o modal e aguarda resposta
                    if (dialog.ShowDialog() == true)
                    {
                        if (int.TryParse(dialog.Answer, out int qtdRemover) && qtdRemover > 0)
                        {
                            if (qtdRemover >= itemRemover.Quantidade)
                            {
                                Carrinho.Remove(itemRemover); // Remove tudo
                            }
                            else
                            {
                                itemRemover.Quantidade -= qtdRemover; // Desconta apenas a quantia
                            }
                            AtualizarTotal();
                        }
                        else
                        {
                            MessageBox.Show("Quantidade inválida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    // Se só tem 1 no carrinho, remove direto sem perguntar
                    Carrinho.Remove(itemRemover);
                    AtualizarTotal();
                }
            }
        }

        private void AtualizarTotal()
        {
            decimal total = Carrinho.Sum(i => i.Subtotal);
            TxtTotal.Text = total.ToString("C"); // Formato de moeda local (R$)
        }

        private void BtnFinalizarVenda_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCaixaAberto) return;

            if (Carrinho.Count == 0)
            {
                MessageBox.Show("O carrinho está vazio!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal totalVenda = Carrinho.Sum(i => i.Subtotal);

            // Abre a janela de pagamento
            var pagamentoDialog = new PagamentoDialog(totalVenda);
            if (pagamentoDialog.ShowDialog() == true)
            {
                // Descontar do estoque
                foreach (var item in Carrinho)
                {
                    var produto = AppState.Produtos.FirstOrDefault(p => p.Id == item.Produto.Id);
                    if (produto != null)
                    {
                        produto.QuantidadeEstoque -= item.Quantidade;
                    }
                }
                
                // Salva as alterações de estoque no banco de dados
                AppState.SalvarAlteracoes();

                // Imprime o recibo automaticamente contendo a forma de pagamento selecionada
                ImpressoraRecibo.ImprimirRecibo(Carrinho, totalVenda, pagamentoDialog.MetodoSelecionado);
                
                Carrinho.Clear();
                AtualizarTotal();
            }
        }

        private void BtnCancelarVenda_Click(object sender, RoutedEventArgs e)
        {
            if (Carrinho.Count > 0)
            {
                var result = MessageBox.Show("Deseja realmente cancelar a venda e limpar o carrinho?", "Cancelar Venda", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Carrinho.Clear();
                    AtualizarTotal();
                }
            }
        }

        // --- LÓGICA DO LEITOR DE CÓDIGO DE BARRAS ---

        private void TxtLeitorCodigo_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtLeitorCodigo.Text == "Clique aqui para o leitor de barras...")
            {
                TxtLeitorCodigo.Text = "";
            }
        }

        private void TxtLeitorCodigo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLeitorCodigo.Text))
            {
                TxtLeitorCodigo.Text = "Clique aqui para o leitor de barras...";
            }
        }

        private void TxtLeitorCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isCaixaAberto) return;

            // O leitor de código de barras geralmente envia a tecla 'Enter' ao final da leitura
            if (e.Key == Key.Enter)
            {
                string codigoLido = TxtLeitorCodigo.Text.Trim();
                
                if (!string.IsNullOrEmpty(codigoLido))
                {
                    // Buscar o produto pelo código de barras
                    var produto = AppState.Produtos.FirstOrDefault(p => p.CodigoBarras == codigoLido);

                    if (produto != null)
                    {
                        // Adicionar ao carrinho reutilizando a lógica existente
                        AdicionarProdutoAoCarrinho(produto);
                        
                        // Limpar para a próxima leitura
                        TxtLeitorCodigo.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Produto não encontrado pelo código: " + codigoLido, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtLeitorCodigo.SelectAll();
                    }
                }
            }
        }

        private void AdicionarProdutoAoCarrinho(Produto produtoSelecionado)
        {
            int quantidadeDesejada = _multiplicadorProximoItem;

            // Verificar se tem em estoque
            if (produtoSelecionado.QuantidadeEstoque <= 0)
            {
                MessageBox.Show("Produto sem estoque!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                _multiplicadorProximoItem = 1; // Reseta o multiplicador
                return;
            }

            // Verificar se já existe no carrinho
            var itemExistente = Carrinho.FirstOrDefault(i => i.Produto.Id == produtoSelecionado.Id);
            int quantidadeNoCarrinho = itemExistente?.Quantidade ?? 0;

            if (quantidadeNoCarrinho + quantidadeDesejada > produtoSelecionado.QuantidadeEstoque)
            {
                MessageBox.Show($"Quantidade máxima em estoque atingida!\nEstoque: {produtoSelecionado.QuantidadeEstoque} | No Carrinho: {quantidadeNoCarrinho} | Tentando Adicionar: {quantidadeDesejada}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                _multiplicadorProximoItem = 1; // Reseta o multiplicador
                return;
            }

            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidadeDesejada;
            }
            else
            {
                Carrinho.Add(new ItemVenda { Produto = produtoSelecionado, Quantidade = quantidadeDesejada });
            }

            // Reseta o multiplicador
            _multiplicadorProximoItem = 1;

            AtualizarTotal();
        }

        // --- ATALHOS PÚBLICOS PARA A MAINWINDOW ---

        public void ProcessarAtalhoF1()
        {
            if (!_isCaixaAberto) return;

            var dialog = new AdicionarMultiploDialog();
            if (dialog.ShowDialog() == true)
            {
                string codigoLido = dialog.CodigoBarrasLido;
                int quantidade = dialog.Quantidade;

                var produto = AppState.Produtos.FirstOrDefault(p => p.CodigoBarras == codigoLido);

                if (produto != null)
                {
                    _multiplicadorProximoItem = quantidade;
                    AdicionarProdutoAoCarrinho(produto);
                }
                else
                {
                    MessageBox.Show("Produto não encontrado pelo código: " + codigoLido, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            TxtLeitorCodigo.Focus();
        }

        public void ProcessarAtalhoF2()
        {
            if (!_isCaixaAberto) return;

            if (Carrinho.Count == 0)
            {
                MessageBox.Show("O carrinho está vazio. Adicione itens antes de dar desconto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new DescontoDialog(Carrinho);
            if (dialog.ShowDialog() == true && dialog.ItemSelecionado != null)
            {
                dialog.ItemSelecionado.Desconto = dialog.DescontoAplicado;
                AtualizarTotal();
            }
        }

        public void AlternarCaixa()
        {
            _isCaixaAberto = !_isCaixaAberto;
            OverlayCaixaFechado.Visibility = _isCaixaAberto ? Visibility.Collapsed : Visibility.Visible;
            
            if (_isCaixaAberto)
            {
                TxtLeitorCodigo.Focus();
            }
        }

        public void ProcessarAtalhoEspaco()
        {
            BtnFinalizarVenda_Click(this, new RoutedEventArgs());
        }
    }
}
