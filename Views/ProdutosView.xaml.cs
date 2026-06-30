using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PDVModerno.Models;

namespace PDVModerno.Views
{
    public partial class ProdutosView : UserControl
    {
        private Produto _produtoEmEdicao = null;

        public ProdutosView()
        {
            InitializeComponent();
            
            // Vincular tabela com o AppState global
            GridProdutos.ItemsSource = AppState.Produtos;
            
            AtualizarCategorias();
        }

        private void AtualizarCategorias()
        {
            var categorias = AppState.Produtos
                                .Where(p => !string.IsNullOrEmpty(p.Categoria))
                                .Select(p => p.Categoria)
                                .Distinct()
                                .OrderBy(c => c)
                                .ToList();
            
            CmbCategoria.ItemsSource = categorias;
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtNome.Text))
                {
                    MessageBox.Show("Informe o nome do produto.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(TxtPreco.Text, out decimal preco))
                {
                    MessageBox.Show("Preço de Venda inválido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                decimal precoCusto = 0;
                if (!string.IsNullOrWhiteSpace(TxtPrecoCusto.Text) && !decimal.TryParse(TxtPrecoCusto.Text, out precoCusto))
                {
                    MessageBox.Show("Preço de Custo inválido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TxtEstoque.Text, out int estoque))
                {
                    MessageBox.Show("Estoque inválido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string codigoBarrasDigitado = TxtCodigoBarras.Text.Trim();
                if (!string.IsNullOrEmpty(codigoBarrasDigitado))
                {
                    // Verifica se já existe um produto com o mesmo código de barras (ignorando o próprio produto se estivermos editando)
                    bool codigoJaExiste = AppState.Produtos.Any(p => p.CodigoBarras == codigoBarrasDigitado && (_produtoEmEdicao == null || p.Id != _produtoEmEdicao.Id));
                    if (codigoJaExiste)
                    {
                        MessageBox.Show($"Já existe um produto cadastrado com o código de barras '{codigoBarrasDigitado}'!", "Código Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtCodigoBarras.Focus();
                        TxtCodigoBarras.SelectAll();
                        return;
                    }
                }

                if (_produtoEmEdicao == null)
                {
                    // MODO CADASTRO
                    int novoId = AppState.Produtos.Count > 0 ? AppState.Produtos.Max(p => p.Id) + 1 : 1;

                    var novoProduto = new Produto
                    {
                        Id = novoId,
                        Nome = TxtNome.Text,
                        Preco = preco,
                        PrecoCusto = precoCusto,
                        QuantidadeEstoque = estoque,
                        Categoria = string.IsNullOrWhiteSpace(CmbCategoria.Text) ? "Geral" : CmbCategoria.Text.Trim(),
                        CodigoBarras = codigoBarrasDigitado
                    };

                    AppState.Db.Produtos.Add(novoProduto);
                    AppState.SalvarAlteracoes();
                    MessageBox.Show("Produto cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // MODO EDIÇÃO
                    _produtoEmEdicao.Nome = TxtNome.Text;
                    _produtoEmEdicao.Preco = preco;
                    _produtoEmEdicao.PrecoCusto = precoCusto;
                    _produtoEmEdicao.QuantidadeEstoque = estoque;
                    _produtoEmEdicao.Categoria = string.IsNullOrWhiteSpace(CmbCategoria.Text) ? "Geral" : CmbCategoria.Text.Trim();
                    _produtoEmEdicao.CodigoBarras = codigoBarrasDigitado;

                    AppState.SalvarAlteracoes();
                    MessageBox.Show("Produto atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    SairModoEdicao();
                }
                
                // Atualiza a lista de categorias no combobox
                AtualizarCategorias();

                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimparCampos()
        {
            TxtCodigoBarras.Clear();
            TxtNome.Clear();
            TxtPrecoCusto.Clear();
            TxtPreco.Clear();
            TxtEstoque.Clear();
            CmbCategoria.Text = string.Empty;
        }

        private void SairModoEdicao()
        {
            _produtoEmEdicao = null;
            BtnSalvar.Content = "SALVAR PRODUTO";
            BtnCancelarEdicao.Visibility = Visibility.Collapsed;
            LimparCampos();
        }

        private void BtnCancelarEdicao_Click(object sender, RoutedEventArgs e)
        {
            SairModoEdicao();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Produto produtoEditar)
            {
                _produtoEmEdicao = produtoEditar;
                
                // Preencher campos
                TxtCodigoBarras.Text = _produtoEmEdicao.CodigoBarras;
                TxtNome.Text = _produtoEmEdicao.Nome;
                TxtPrecoCusto.Text = _produtoEmEdicao.PrecoCusto > 0 ? _produtoEmEdicao.PrecoCusto.ToString("0.00") : "";
                TxtPreco.Text = _produtoEmEdicao.Preco.ToString("0.00");
                TxtEstoque.Text = _produtoEmEdicao.QuantidadeEstoque.ToString();
                CmbCategoria.Text = _produtoEmEdicao.Categoria;

                // Mudar botões
                BtnSalvar.Content = "ATUALIZAR PRODUTO";
                BtnCancelarEdicao.Visibility = Visibility.Visible;
            }
        }

        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Produto produtoRemover)
            {
                var result = MessageBox.Show($"Deseja realmente excluir o produto {produtoRemover.Nome}?", "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    AppState.Db.Produtos.Remove(produtoRemover);
                    AppState.SalvarAlteracoes();
                }
            }
        }
    }
}
