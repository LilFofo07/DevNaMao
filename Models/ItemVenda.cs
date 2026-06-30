using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PDVModerno.Models
{
    public class ItemVenda : INotifyPropertyChanged
    {
        private Produto produto;
        private int quantidade;
        private decimal desconto;

        public Produto Produto
        {
            get => produto;
            set 
            { 
                produto = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Subtotal)); 
            }
        }

        public int Quantidade
        {
            get => quantidade;
            set 
            { 
                quantidade = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Subtotal)); 
            }
        }

        public decimal Desconto
        {
            get => desconto;
            set 
            { 
                desconto = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Subtotal)); 
            }
        }

        public decimal Subtotal => ((Produto?.Preco ?? 0) * Quantidade) - Desconto;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
