using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PDVModerno.Views
{
    public partial class SuporteView : UserControl
    {
        private static readonly HttpClient _httpClient = new();
        private string _cnpj = string.Empty;
        private string _centralUrl = string.Empty;
        private DispatcherTimer _pollTimer;

        public ObservableCollection<LocalChatMessage> Mensagens { get; set; } = new();

        public SuporteView()
        {
            InitializeComponent();
            ListMensagens.ItemsSource = Mensagens;

            CarregarConfig();

            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromSeconds(2);
            _pollTimer.Tick += async (s, e) => await CarregarMensagensAsync();

            this.Loaded += (s, e) => _pollTimer.Start();
            this.Unloaded += (s, e) => _pollTimer.Stop();
        }

        private void CarregarConfig()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
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
                    }
                }
            }
            catch
            {
                // Ignora falhas silenciosamente
            }
        }

        private async Task CarregarMensagensAsync()
        {
            if (string.IsNullOrWhiteSpace(_centralUrl) || string.IsNullOrWhiteSpace(_cnpj))
            {
                BorderStatus.Visibility = Visibility.Visible;
                TxtStatusConexao.Text = "CNPJ ou URL Central não configurados no config.json.";
                return;
            }

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(2));
                string url = $"{_centralUrl.TrimEnd('/')}/chat?cnpj={_cnpj}";

                HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    BorderStatus.Visibility = Visibility.Collapsed;
                    string content = await response.Content.ReadAsStringAsync();

                    var remoteMsgs = JsonSerializer.Deserialize<LocalChatMessage[]>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (remoteMsgs != null && remoteMsgs.Length != Mensagens.Count)
                    {
                        Mensagens.Clear();
                        foreach (var msg in remoteMsgs)
                        {
                            Mensagens.Add(msg);
                        }
                        ScrollToBottom();
                    }
                }
                else
                {
                    BorderStatus.Visibility = Visibility.Visible;
                    TxtStatusConexao.Text = $"Conexão recusada pela Central (Status HTTP: {(int)response.StatusCode})";
                }
            }
            catch
            {
                BorderStatus.Visibility = Visibility.Visible;
                TxtStatusConexao.Text = "Central de Suporte Offline - Abra o painel PDVCentral para conectar";
            }
        }

        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            await EnviarMensagemAsync();
        }

        private async void TxtMensagem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await EnviarMensagemAsync();
            }
        }

        private async Task EnviarMensagemAsync()
        {
            string message = TxtMensagem.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            if (string.IsNullOrWhiteSpace(_centralUrl) || string.IsNullOrWhiteSpace(_cnpj))
            {
                MessageBox.Show("Configuração CNPJ ou Central inválida no config.json.", "Configuração", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string url = $"{_centralUrl.TrimEnd('/')}/chat";
                var payload = new
                {
                    cnpj = _cnpj,
                    sender = "Cliente",
                    message = message
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    TxtMensagem.Clear();
                    await CarregarMensagensAsync();
                }
                else
                {
                    MessageBox.Show($"Falha ao enviar mensagem: HTTP {(int)response.StatusCode}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sem conexão com o painel central de suporte: {ex.Message}", "Falha de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ScrollMessages.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TxtMensagem_GotFocus(object sender, RoutedEventArgs e)
        {
            ScrollToBottom();
        }

        private void TxtMensagem_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ScrollToBottom();
        }
    }

    public class LocalChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
