using System.Windows.Controls;
using PDVModerno.Models;

namespace PDVModerno.Views
{
    public partial class EstoqueView : UserControl
    {
        public EstoqueView()
        {
            InitializeComponent();
            
            // Vincular tabela com o AppState global
            GridEstoque.ItemsSource = AppState.Produtos;
        }
    }
}
