using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PDVModerno.Models
{
    public class Produto : INotifyPropertyChanged
    {
        private int id;
        private string nome = string.Empty;
        private decimal preco;
        private int quantidadeEstoque;
        private string categoria = "Geral";
        private string imagemUrl = string.Empty;
        private string codigoBarras = string.Empty;
        private decimal precoCusto;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(); }
        }

        public string CodigoBarras
        {
            get => codigoBarras;
            set { codigoBarras = value; OnPropertyChanged(); }
        }

        public string Nome
        {
            get => nome;
            set { nome = value; OnPropertyChanged(); }
        }

        public decimal PrecoCusto
        {
            get => precoCusto;
            set { precoCusto = value; OnPropertyChanged(); }
        }

        public decimal Preco
        {
            get => preco;
            set { preco = value; OnPropertyChanged(); }
        }

        public int QuantidadeEstoque
        {
            get => quantidadeEstoque;
            set { quantidadeEstoque = value; OnPropertyChanged(); }
        }

        public string Categoria
        {
            get => categoria;
            set { categoria = value; OnPropertyChanged(); }
        }

        public string ImagemUrl
        {
            get => imagemUrl;
            set { imagemUrl = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
