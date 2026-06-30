using System;

namespace PDVCentral.Models
{
    public class VendaMonitorada
    {
        public int Id { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty; // "PIX", "Dinheiro", etc
        public string Status { get; set; } = string.Empty; // "approved", etc
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
