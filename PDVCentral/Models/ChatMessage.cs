using System;

namespace PDVCentral.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty; // "Cliente", "Operador"
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
