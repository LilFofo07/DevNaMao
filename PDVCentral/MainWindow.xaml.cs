using System;
using System.Windows;
using PDVCentral.Utils;
using PDVCentral.Views;

namespace PDVCentral
{
    public partial class MainWindow : Window
    {
        private DashboardView? _dashboardView;
        private ClientesView? _clientesView;
        private ChatView? _chatView;
        private MaintenanceView? _maintenanceView;

        private System.Windows.Threading.DispatcherTimer? _apiPingTimer;
        private static readonly System.Net.Http.HttpClient _httpClient = new();

        public MainWindow()
        {
            InitializeComponent();
            
            // Iniciar timer para verificar status da API remota
            _apiPingTimer = new System.Windows.Threading.DispatcherTimer();
            _apiPingTimer.Interval = TimeSpan.FromSeconds(5);
            _apiPingTimer.Tick += async (s, e) => await VerificarConexaoApiAsync();
            _apiPingTimer.Start();

            _ = VerificarConexaoApiAsync();

            // Exibir a tela padrão (Dashboard)
            ShowView("Dashboard");
        }

        private async System.Threading.Tasks.Task VerificarConexaoApiAsync()
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(2));
                string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes";
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    ServerStatusLed.Fill = (System.Windows.Media.Brush)FindResource("SuccessColor");
                    ServerStatusText.Text = "API Remota Online";
                }
                else
                {
                    ServerStatusLed.Fill = (System.Windows.Media.Brush)FindResource("DangerColor");
                    ServerStatusText.Text = "API Remota com Erro";
                }
            }
            catch
            {
                ServerStatusLed.Fill = (System.Windows.Media.Brush)FindResource("DangerColor");
                ServerStatusText.Text = "API Remota Offline";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _apiPingTimer?.Stop();
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            if (BtnDashboard.IsChecked == true)
            {
                ShowView("Dashboard");
            }
            else if (BtnClientes.IsChecked == true)
            {
                ShowView("Clientes");
            }
            else if (BtnChat.IsChecked == true)
            {
                ShowView("Chat");
            }
            else if (BtnManutencao.IsChecked == true)
            {
                ShowView("Manutencao");
            }
        }

        private void ShowView(string viewName)
        {
            switch (viewName)
            {
                case "Dashboard":
                    _dashboardView ??= new DashboardView();
                    MainContent.Content = _dashboardView;
                    _dashboardView.LoadData();
                    break;
                case "Clientes":
                    _clientesView ??= new ClientesView();
                    MainContent.Content = _clientesView;
                    _clientesView.LoadData();
                    break;
                case "Chat":
                    _chatView ??= new ChatView();
                    MainContent.Content = _chatView;
                    _chatView.LoadData();
                    break;
                case "Manutencao":
                    _maintenanceView ??= new MaintenanceView();
                    MainContent.Content = _maintenanceView;
                    _maintenanceView.RefreshDatabaseInfo();
                    break;
            }
        }
    }
}
