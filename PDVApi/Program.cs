using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS para permitir chamadas de qualquer origem (WPF e navegadores)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Registrar o DbContext da API
builder.Services.AddDbContext<ApiDbContext>();

var app = builder.Build();

app.UseCors();

// Garantir criação do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}

// ROTA: Obter token Mercado Pago do Cliente
app.MapGet("/config", async (string cnpj, ApiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(cnpj)) return Results.BadRequest("CNPJ não fornecido.");

    var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Cnpj == cleanCnpj);

    if (cliente == null) return Results.NotFound("Cliente não cadastrado.");
    if (cliente.Status == "Bloqueado") return Results.Json(new { error = "Cliente suspenso." }, statusCode: 403);

    return Results.Ok(new { accessToken = cliente.MercadoPagoToken });
});

// ROTAS: Clientes (CRUD completo)
app.MapGet("/api/clientes", async (ApiDbContext db) =>
{
    var lista = await db.Clientes.OrderBy(c => c.Nome).ToListAsync();
    return Results.Ok(lista);
});

app.MapPost("/api/clientes", async (Cliente cliente, ApiDbContext db) =>
{
    if (cliente == null || string.IsNullOrWhiteSpace(cliente.Cnpj) || string.IsNullOrWhiteSpace(cliente.Nome))
    {
        return Results.BadRequest("Dados de cliente inválidos.");
    }

    cliente.Cnpj = cliente.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var existente = await db.Clientes.FirstOrDefaultAsync(c => c.Cnpj == cliente.Cnpj);

    if (existente == null)
    {
        cliente.DataCriacao = DateTime.Now;
        db.Clientes.Add(cliente);
    }
    else
    {
        existente.Nome = cliente.Nome;
        existente.Email = cliente.Email;
        existente.MercadoPagoToken = cliente.MercadoPagoToken;
        existente.Status = cliente.Status;
    }

    await db.SaveChangesAsync();
    return Results.Ok(cliente);
});

app.MapDelete("/api/clientes/{cnpj}", async (string cnpj, ApiDbContext db) =>
{
    var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Cnpj == cleanCnpj);
    if (cliente == null) return Results.NotFound();

    db.Clientes.Remove(cliente);
    
    // Remover histórico de chat
    var msgs = db.ChatMessages.Where(m => m.Cnpj == cleanCnpj);
    db.ChatMessages.RemoveRange(msgs);

    await db.SaveChangesAsync();
    return Results.Ok();
});

// ROTAS: Chat de Suporte
app.MapGet("/chat", async (string cnpj, ApiDbContext db) =>
{
    Console.WriteLine($"[API GET /chat] CNPJ recebido: '{cnpj}'");
    if (string.IsNullOrWhiteSpace(cnpj)) return Results.BadRequest();
    
    var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var messages = await db.ChatMessages
        .Where(m => m.Cnpj == cleanCnpj)
        .OrderBy(m => m.Timestamp)
        .ToListAsync();

    Console.WriteLine($"[API GET /chat] Mensagens encontradas para CNPJ '{cleanCnpj}': {messages.Count}");
    return Results.Ok(messages);
});

app.MapPost("/chat", async (HttpContext httpContext, ApiDbContext db) =>
{
    using var reader = new StreamReader(httpContext.Request.Body);
    string jsonString = await reader.ReadToEndAsync();
    
    Console.WriteLine($"[API POST /chat] JSON recebido: {jsonString}");
    
    using var doc = JsonDocument.Parse(jsonString);
    var root = doc.RootElement;
    
    string cnpj = "";
    string sender = "";
    string message = "";
    
    foreach (var prop in root.EnumerateObject())
    {
        if (prop.Name.Equals("cnpj", StringComparison.OrdinalIgnoreCase))
            cnpj = prop.Value.GetString() ?? "";
        else if (prop.Name.Equals("sender", StringComparison.OrdinalIgnoreCase))
            sender = prop.Value.GetString() ?? "";
        else if (prop.Name.Equals("message", StringComparison.OrdinalIgnoreCase))
            message = prop.Value.GetString() ?? "";
    }

    if (string.IsNullOrWhiteSpace(cnpj) || string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine("[API POST /chat] BAD REQUEST: CNPJ ou Mensagem vazios.");
        return Results.BadRequest();
    }

    var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var msg = new ChatMessage
    {
        Cnpj = cleanCnpj,
        Sender = string.IsNullOrWhiteSpace(sender) ? "Cliente" : sender,
        Message = message,
        Timestamp = DateTime.Now
    };

    db.ChatMessages.Add(msg);
    await db.SaveChangesAsync();
    Console.WriteLine($"[API POST /chat] Mensagem persistida para CNPJ {cleanCnpj}!");
    return Results.Ok(msg);
});

// ROTAS: Vendas Monitoradas
app.MapGet("/api/vendas", async (ApiDbContext db) =>
{
    var vendas = await db.VendasMonitoradas.OrderByDescending(v => v.Timestamp).Take(50).ToListAsync();
    return Results.Ok(vendas);
});

app.MapPost("/venda", async (HttpContext httpContext, ApiDbContext db) =>
{
    using var reader = new StreamReader(httpContext.Request.Body);
    string jsonString = await reader.ReadToEndAsync();
    
    Console.WriteLine($"[API POST /venda] JSON recebido: {jsonString}");
    
    using var doc = JsonDocument.Parse(jsonString);
    var root = doc.RootElement;
    
    string cnpj = "";
    decimal valor = 0;
    string metodo = "";
    string status = "";
    
    foreach (var prop in root.EnumerateObject())
    {
        if (prop.Name.Equals("cnpj", StringComparison.OrdinalIgnoreCase))
            cnpj = prop.Value.GetString() ?? "";
        else if (prop.Name.Equals("valor", StringComparison.OrdinalIgnoreCase))
            valor = prop.Value.GetDecimal();
        else if (prop.Name.Equals("metodo", StringComparison.OrdinalIgnoreCase))
            metodo = prop.Value.GetString() ?? "";
        else if (prop.Name.Equals("status", StringComparison.OrdinalIgnoreCase))
            status = prop.Value.GetString() ?? "";
    }

    if (string.IsNullOrWhiteSpace(cnpj))
    {
        Console.WriteLine("[API POST /venda] BAD REQUEST: CNPJ vazio.");
        return Results.BadRequest();
    }

    var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
    var venda = new VendaMonitorada
    {
        Cnpj = cleanCnpj,
        Valor = valor,
        Metodo = string.IsNullOrWhiteSpace(metodo) ? "Outro" : metodo,
        Status = string.IsNullOrWhiteSpace(status) ? "approved" : status,
        Timestamp = DateTime.Now
    };

    db.VendasMonitoradas.Add(venda);
    await db.SaveChangesAsync();
    Console.WriteLine($"[API POST /venda] Venda de R$ {valor} persistida para CNPJ {cleanCnpj}!");
    return Results.Ok(venda);
});

// ROTA: Estatísticas Gerais do Dashboard
app.MapGet("/api/dashboard-stats", async (ApiDbContext db) =>
{
    int totalClientes = await db.Clientes.CountAsync();
    decimal totalVendas = await db.VendasMonitoradas.SumAsync(v => v.Valor);
    int totalTransacoes = await db.VendasMonitoradas.CountAsync();
    
    var recentes = await db.VendasMonitoradas
        .OrderByDescending(v => v.Timestamp)
        .Take(50)
        .ToListAsync();

    return Results.Ok(new
    {
        totalClientes,
        totalVendas,
        totalTransacoes,
        vendasRecentes = recentes
    });
});

app.Run("http://localhost:5080");

// --- DEFINIÇÕES DE BANCO DE DADOS & ENTIDADES ---

public class ApiDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<VendaMonitorada> VendasMonitoradas { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "banco_api.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}

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

public class ChatMessage
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty; // "Cliente", "Operador"
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class VendaMonitorada
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
