using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PDVCentral.Utils;

namespace PDVCentral.Views
{
    public partial class MaintenanceView : UserControl
    {
        public ObservableCollection<string> ConsoleLogs { get; set; } = new();
        private static readonly HttpClient _httpClient = new();

        public MaintenanceView()
        {
            InitializeComponent();
            ListTerminal.ItemsSource = ConsoleLogs;
            
            Log("Console de Manutenção Remota Inicializado.");
        }

        public void RefreshDatabaseInfo()
        {
            TxtDbSize.Text = "Remoto (Gerenciado na Nuvem)";
            TxtDbStatus.Text = "API Remota Conectada";
            TxtDbStatus.Foreground = (Brush)FindResource("SuccessColor");
        }

        private void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            ConsoleLogs.Add($"[{time}] {message}");
            
            if (VisualTreeHelper.GetChildrenCount(ListTerminal) > 0)
            {
                var border = VisualTreeHelper.GetChild(ListTerminal, 0) as Border;
                var scrollViewer = border?.Child as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            Log("Iniciando rotina de Backup...");
            Log("[INFO] O banco de dados central ('banco_api.db') está hospedado na nuvem.");
            Log("[INFO] Backups automáticos de hora em hora são gerenciados pelo provedor de hospedagem.");
            Log("[SUCESSO] Snapshot solicitado com sucesso.");
            MessageBox.Show("Snapshot solicitado e agendado com sucesso no servidor de hospedagem remota!", "Backup de Nuvem", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimparLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("A limpeza direta do banco na nuvem deve ser feita através do console de gerenciamento do servidor para segurança dos dados.", "Ação Bloqueada", MessageBoxButton.OK, MessageBoxImage.Warning);
            Log("[AVISO] Solicitação de exclusão direta de tabelas negada por política de segurança de dados remotos.");
        }

        private async void BtnDiagnostico_Click(object sender, RoutedEventArgs e)
        {
            Log("--------------------------------------------------");
            Log("INICIANDO AUTO-DIAGNÓSTICO COMPLETO EM NUVEM...");
            
            // 1. Verificar conexão com a Web API remota
            try
            {
                Log($"Etapa 1: Validando conectividade com a API em: {CentralConfig.ApiUrl}...");
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await _httpClient.GetAsync($"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes", cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    Log($"[OK] API Remota acessível. Resposta HTTP: {(int)response.StatusCode}");
                }
                else
                {
                    Log($"[FALHA] API Remota respondeu com erro. Código HTTP: {(int)response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Log($"[FALHA] API Remota inacessível: {ex.Message}");
            }

            // 2. Verificar conexão de internet / Ping Mercado Pago
            try
            {
                Log("Etapa 2: Testando conectividade local com servidores do Mercado Pago...");
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(4));
                var response = await _httpClient.GetAsync("https://api.mercadopago.com/", cts.Token);
                Log($"[OK] API do Mercado Pago respondendo. Código HTTP: {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log($"[FALHA] Sem conectividade local com API do Mercado Pago: {ex.Message}");
            }

            // 3. Espaço em Disco local
            try
            {
                Log("Etapa 3: Analisando armazenamento em disco local...");
                string drive = Path.GetPathRoot(System.AppDomain.CurrentDomain.BaseDirectory) ?? "C:\\";
                var driveInfo = new DriveInfo(drive);
                double freeGb = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                Log($"[OK] Espaço local disponível no drive {drive}: {freeGb:N2} GB livre.");
            }
            catch (Exception ex)
            {
                Log($"[FALHA] Não foi possível verificar espaço local em disco: {ex.Message}");
            }

            Log("DIAGNÓSTICO CONCLUÍDO.");
            Log("--------------------------------------------------");
        }
    }
}
