using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PDVModerno.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // O banco será criado na mesma pasta do executável
            string dbPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "banco.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
