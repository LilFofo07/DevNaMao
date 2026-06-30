using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PDVModerno.Models
{
    public static class AppState
    {
        public static ObservableCollection<Produto> Produtos { get; set; }
        
        public static AppDbContext Db { get; private set; }

        static AppState()
        {
            Db = new AppDbContext();
            
            // Cria o arquivo do banco de dados (se não existir) com as tabelas
            Db.Database.EnsureCreated();
            
            // Carrega os dados do banco para a memória para fins de UI rápida
            Db.Produtos.Load();
            
            // Atribui a coleção local do EF para a nossa UI
            Produtos = Db.Produtos.Local.ToObservableCollection();

            // Popular se estiver vazio na primeira vez
            if (Produtos.Count == 0)
            {
                var produtoExemplo = new Produto { Nome = "Produto Teste", Preco = 10.00m, QuantidadeEstoque = 50, Categoria = "Geral", CodigoBarras = "0000" };
                Db.Produtos.Add(produtoExemplo);
                Db.SaveChanges();
            }
        }

        public static void SalvarAlteracoes()
        {
            Db.SaveChanges();
        }
    }
}
