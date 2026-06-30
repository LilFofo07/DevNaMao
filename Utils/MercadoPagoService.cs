using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDVModerno.Utils
{
    public static class MercadoPagoService
    {
        private static string _accessToken = string.Empty;
        private static string _cnpj = string.Empty;
        private static string _centralUrl = string.Empty;
        private static string _fallbackToken = string.Empty;
        private static readonly HttpClient _httpClient = new HttpClient();

        static MercadoPagoService()
        {
            CarregarConfig();
        }

        public static void CarregarConfig()
        {
            try
            {
                // Tenta carregar do diretório do executável
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                
                // Se não existir, tenta subir um nível (caso esteja rodando no VS/debug)
                if (!File.Exists(path))
                {
                    path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName ?? "", "config.json");
                }

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("Cnpj", out JsonElement cnpjElem))
                        {
                            _cnpj = cnpjElem.GetString() ?? string.Empty;
                        }
                        if (root.TryGetProperty("CentralUrl", out JsonElement centralUrlElem))
                        {
                            _centralUrl = centralUrlElem.GetString() ?? string.Empty;
                        }
                        if (root.TryGetProperty("MercadoPagoAccessToken", out JsonElement tokenElement))
                        {
                            _fallbackToken = tokenElement.GetString() ?? string.Empty;
                            _accessToken = _fallbackToken;
                        }
                    }
                }
            }
            catch
            {
                // Ignora falhas silenciosamente
            }
        }

        public static async Task<string> ObterTokenAtualizadoAsync()
        {
            if (string.IsNullOrWhiteSpace(_centralUrl) || string.IsNullOrWhiteSpace(_cnpj))
            {
                return _fallbackToken;
            }

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(2));
                string url = $"{_centralUrl.TrimEnd('/')}/config?cnpj={_cnpj}";
                
                HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    string remoteToken = doc.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(remoteToken))
                    {
                        _accessToken = remoteToken;
                        return remoteToken;
                    }
                }
            }
            catch
            {
                // Se o servidor central estiver offline ou houver erro, usa o token de fallback local
            }

            return _fallbackToken;
        }

        public static async Task NotificarVendaCentralAsync(decimal valor, string metodo, string status)
        {
            if (string.IsNullOrWhiteSpace(_centralUrl) || string.IsNullOrWhiteSpace(_cnpj))
            {
                return;
            }

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));
                string url = $"{_centralUrl.TrimEnd('/')}/venda";

                var payload = new
                {
                    cnpj = _cnpj,
                    valor = valor,
                    metodo = metodo,
                    status = status
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(url, content, cts.Token);
            }
            catch
            {
                // Falha silenciosa para não quebrar a experiência do PDV se o Central cair
            }
        }

        public static async Task<(string QrCode, string QrCodeBase64, long PaymentId, string Error)> CriarPagamentoPixAsync(decimal valor)
        {
            _accessToken = await ObterTokenAtualizadoAsync();

            if (string.IsNullOrWhiteSpace(_accessToken) || _accessToken == "INSIRA_SEU_ACCESS_TOKEN_AQUI")
            {
                return (null, null, 0, "Access Token do Mercado Pago não configurado.");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/v1/payments");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

                var payload = new
                {
                    transaction_amount = valor,
                    description = "Venda PDV",
                    payment_method_id = "pix",
                    payer = new
                    {
                        email = "edwilsondddss@gmail.com",
                        first_name = "Cliente",
                        last_name = "PDV"
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null, 0, $"Erro MP ({response.StatusCode}): {responseBody}");
                }

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;
                    long paymentId = root.GetProperty("id").GetInt64();
                    
                    JsonElement pointOfInteraction = root.GetProperty("point_of_interaction");
                    JsonElement transactionData = pointOfInteraction.GetProperty("transaction_data");
                    
                    string qrCode = transactionData.GetProperty("qr_code").GetString() ?? "";
                    string qrCodeBase64 = transactionData.GetProperty("qr_code_base64").GetString() ?? "";

                    return (qrCode, qrCodeBase64, paymentId, null);
                }
            }
            catch (Exception ex)
            {
                return (null, null, 0, $"Falha de conexão com a API: {ex.Message}");
            }
        }

        public static async Task<(string Status, string Error)> ConsultarStatusPagamentoAsync(long paymentId)
        {
            _accessToken = await ObterTokenAtualizadoAsync();

            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                return (null, "Access Token não configurado.");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{paymentId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (null, $"Erro MP ({response.StatusCode}): {responseBody}");
                }

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    string status = doc.RootElement.GetProperty("status").GetString() ?? "pending";
                    return (status, null);
                }
            }
            catch (Exception ex)
            {
                return (null, $"Erro de conexão ao consultar status: {ex.Message}");
            }
        }
    }
}
