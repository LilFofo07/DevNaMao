using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PDVCentral.Models;
using PDVCentral.Utils;

namespace PDVCentral.Views
{
    public partial class DashboardView : UserControl
    {
        private static readonly HttpClient _httpClient = new();
        private DispatcherTimer? _refreshTimer;
        public ObservableCollection<VendaMonitorada> Vendas { get; set; } = new();

        public DashboardView()
        {
            InitializeComponent();
            GridVendas.ItemsSource = Vendas;

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(3);
            _refreshTimer.Tick += async (s, e) => await CarregarDadosApiAsync();

            this.Loaded += (s, e) =>
            {
                _refreshTimer.Start();
                LoadData();
            };
            this.Unloaded += (s, e) => _refreshTimer.Stop();
        }

        public void LoadData()
        {
            _ = CarregarDadosApiAsync();
        }

        private async System.Threading.Tasks.Task CarregarDadosApiAsync()
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(2.5));
                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/dashboard-stats";
                
                HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    int totalClientes = root.GetProperty("totalClientes").GetInt32();
                    decimal totalVendas = root.GetProperty("totalVendas").GetDecimal();
                    int totalTransacoes = root.GetProperty("totalTransacoes").GetInt32();

                    TxtTotalClientes.Text = totalClientes.ToString();
                    TxtVolumeTotal.Text = string.Format(System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), "{0:C}", totalVendas);
                    TxtTotalTransacoes.Text = totalTransacoes.ToString();

                    var vendasElem = root.GetProperty("vendasRecentes");
                    var recentes = JsonSerializer.Deserialize<VendaMonitorada[]>(vendasElem.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (recentes != null)
                    {
                        Vendas.Clear();
                        foreach (var v in recentes)
                        {
                            Vendas.Add(v);
                        }
                    }
                }
            }
            catch
            {
                // Falha silenciosa de conexão no dashboard
            }
        }
    }
}
