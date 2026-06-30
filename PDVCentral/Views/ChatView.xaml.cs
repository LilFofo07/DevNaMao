using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PDVCentral.Models;
using PDVCentral.Utils;

namespace PDVCentral.Views
{
    public partial class ChatView : UserControl
    {
        private static readonly HttpClient _httpClient = new();
        private Cliente? _clienteSelecionado;
        private DispatcherTimer? _pollTimer;

        public ObservableCollection<ChatMessage> Mensagens { get; set; } = new();

        public ChatView()
        {
            InitializeComponent();
            ChatMessagesList.ItemsSource = Mensagens;

            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromSeconds(2);
            _pollTimer.Tick += async (s, e) => await PollingChatAsync();

            this.Loaded += (s, e) =>
            {
                _pollTimer.Start();
                LoadData();
            };
            this.Unloaded += (s, e) => _pollTimer.Stop();
        }

        public async void LoadData()
        {
            try
            {
                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var clientes = JsonSerializer.Deserialize<List<Cliente>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();

                    ListClientesConversa.ItemsSource = clientes.OrderBy(c => c.Nome).ToList();

                    if (_clienteSelecionado != null)
                    {
                        var atual = clientes.FirstOrDefault(c => c.Cnpj == _clienteSelecionado.Cnpj);
                        if (atual != null)
                        {
                            ListClientesConversa.SelectedItem = atual;
                        }
                        else
                        {
                            _clienteSelecionado = null;
                            ChatPanel.Visibility = Visibility.Collapsed;
                            TxtEmptyChat.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch
            {
                // Falha silenciosa de conexão
            }
        }

        private void ListClientesConversa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListClientesConversa.SelectedItem is Cliente cliente)
            {
                _clienteSelecionado = cliente;
                TxtNomeChat.Text = cliente.Nome;

                // Formatação segura de CNPJ para evitar crash se o valor for menor/maior que 14 caracteres
                string cnpj = cliente.Cnpj;
                if (cnpj.Length == 14)
                {
                    TxtCnpjChat.Text = $"CNPJ: {cnpj.Insert(2, ".").Insert(6, ".").Insert(10, "/").Insert(15, "-")}";
                }
                else if (cnpj.Length == 11)
                {
                    TxtCnpjChat.Text = $"CPF: {cnpj.Insert(3, ".").Insert(7, ".").Insert(11, "-")}";
                }
                else
                {
                    TxtCnpjChat.Text = $"CNPJ/CPF: {cnpj}";
                }

                TxtEmptyChat.Visibility = Visibility.Collapsed;
                ChatPanel.Visibility = Visibility.Visible;

                _ = CarregarHistoricoConversaAsync();
            }
            else
            {
                _clienteSelecionado = null;
                ChatPanel.Visibility = Visibility.Collapsed;
                TxtEmptyChat.Visibility = Visibility.Visible;
            }
        }

        private async Task CarregarHistoricoConversaAsync()
        {
            if (_clienteSelecionado == null) return;

            try
            {
                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/chat?cnpj={_clienteSelecionado.Cnpj}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var historico = JsonSerializer.Deserialize<List<ChatMessage>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();

                    if (historico.Count != Mensagens.Count)
                    {
                        Mensagens.Clear();
                        foreach (var msg in historico)
                        {
                            Mensagens.Add(msg);
                        }
                        ScrollToBottom();
                    }
                }
            }
            catch
            {
                // Falha silenciosa
            }
        }

        private async Task PollingChatAsync()
        {
            if (_clienteSelecionado == null) return;
            await CarregarHistoricoConversaAsync();
        }

        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            await EnviarMensagemAsync();
        }

        private async void TxtInputMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await EnviarMensagemAsync();
            }
        }

        private async Task EnviarMensagemAsync()
        {
            if (_clienteSelecionado == null) return;
            string txt = TxtInputMsg.Text.Trim();
            if (string.IsNullOrEmpty(txt)) return;

            try
            {
                var chatMsg = new ChatMessage
                {
                    Cnpj = _clienteSelecionado.Cnpj,
                    Sender = "Operador",
                    Message = txt,
                    Timestamp = DateTime.Now
                };

                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/chat";
                string json = JsonSerializer.Serialize(chatMsg);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    Mensagens.Add(chatMsg);
                    TxtInputMsg.Clear();
                    ScrollToBottom();
                }
                else
                {
                    MessageBox.Show($"Falha ao enviar mensagem à API: HTTP {(int)response.StatusCode}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão: {ex.Message}", "Erro de Rede", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ScrollMessages.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TxtInputMsg_GotFocus(object sender, RoutedEventArgs e)
        {
            ScrollToBottom();
        }
    }
}
