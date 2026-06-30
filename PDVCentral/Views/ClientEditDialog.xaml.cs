using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using PDVCentral.Models;
using PDVCentral.Utils;

namespace PDVCentral.Views
{
    public partial class ClientEditDialog : Window
    {
        private readonly Cliente? _clienteExistente;
        private static readonly HttpClient _httpClient = new();

        public ClientEditDialog(Cliente? cliente)
        {
            InitializeComponent();
            _clienteExistente = cliente;

            if (_clienteExistente != null)
            {
                TxtTitulo.Text = "Editar Cadastro de Cliente";
                TxtNome.Text = _clienteExistente.Nome;
                TxtCnpj.Text = _clienteExistente.Cnpj;
                TxtCnpj.IsEnabled = false; // Não permite alterar a chave primária
                TxtEmail.Text = _clienteExistente.Email;
                TxtToken.Text = _clienteExistente.MercadoPagoToken;
            }
            else
            {
                TxtTitulo.Text = "Cadastrar Novo Cliente";
            }
        }

        private async void BtnTestarToken_Click(object sender, RoutedEventArgs e)
        {
            string token = TxtToken.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                SetResultado("Insira o token para realizar o teste.", false);
                return;
            }

            BtnTestarToken.IsEnabled = false;
            SetResultado("Verificando token na API do Mercado Pago...", null);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.mercadopago.com/users/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    string nickname = doc.RootElement.GetProperty("nickname").GetString() ?? "Desconhecido";
                    
                    SetResultado($"Token Válido! Conta: {nickname}", true);
                }
                else
                {
                    SetResultado($"Erro: Token inválido ou expirado (Código HTTP: {(int)response.StatusCode})", false);
                }
            }
            catch (Exception ex)
            {
                SetResultado($"Erro de conexão: {ex.Message}", false);
            }
            finally
            {
                BtnTestarToken.IsEnabled = true;
            }
        }

        private void SetResultado(string mensagem, bool? sucesso)
        {
            TxtResultadoTeste.Text = mensagem;
            if (sucesso == true)
            {
                TxtResultadoTeste.Foreground = (Brush)FindResource("SuccessColor");
            }
            else if (sucesso == false)
            {
                TxtResultadoTeste.Foreground = (Brush)FindResource("DangerColor");
            }
            else
            {
                TxtResultadoTeste.Foreground = (Brush)FindResource("TextSecondary");
            }
        }

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            string cnpj = TxtCnpj.Text.Trim().Replace(".", "").Replace("/", "").Replace("-", "");
            string nome = TxtNome.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string token = TxtToken.Text.Trim();

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cnpj))
            {
                MessageBox.Show("Nome e CNPJ/CPF são campos obrigatórios.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cliente = new Cliente
                {
                    Cnpj = cnpj,
                    Nome = nome,
                    Email = email,
                    MercadoPagoToken = token,
                    Status = _clienteExistente?.Status ?? "Ativo",
                    DataCriacao = _clienteExistente?.DataCriacao ?? DateTime.Now
                };

                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes";
                string json = JsonSerializer.Serialize(cliente);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Falha ao salvar cliente na API: HTTP {(int)response.StatusCode}", "Erro ao Salvar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão com a API Central: {ex.Message}", "Erro de Rede", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
