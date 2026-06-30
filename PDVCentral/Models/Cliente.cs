using System;
using System.ComponentModel.DataAnnotations;

namespace PDVCentral.Models
{
    public class Cliente
    {
        [Key]
        public string Cnpj { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MercadoPagoToken { get; set; } = string.Empty;
        public string Status { get; set; } = "Ativo"; // "Ativo", "Bloqueado"
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
