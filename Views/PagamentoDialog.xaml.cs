using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using QRCoder;
using PDVModerno.Utils;

namespace PDVModerno.Views
{
    public partial class PagamentoDialog : Window
    {
        private readonly decimal _total;
        private DispatcherTimer _pixPollTimer;
        private long _paymentId;
        private bool _isRealPix = false;

        public string MetodoSelecionado { get; private set; } = "Dinheiro";
        public decimal ValorRecebido { get; private set; }
        public decimal Troco { get; private set; }

        public PagamentoDialog(decimal total)
        {
            InitializeComponent();
            _total = total;
            TxtTotalPagar.Text = _total.ToString("C");
            TxtValorRecebido.Text = _total.ToString("N2"); // Preenche com o total por padrão
            
            // Configurar Polling do Pix
            _pixPollTimer = new DispatcherTimer();
            _pixPollTimer.Interval = TimeSpan.FromSeconds(3);
            _pixPollTimer.Tick += PixPollTimer_Tick;

            // Aceita apenas números e vírgula
            TxtValorRecebido.PreviewTextInput += TxtValorRecebido_PreviewTextInput;
            
            // Foca no campo de valor recebido por padrão
            Loaded += (s, e) => 
            {
                TxtValorRecebido.Focus();
                TxtValorRecebido.SelectAll();
            };

            // Parar o timer ao fechar a janela
            Closed += (s, e) => PararTimer();
        }

        private void TxtValorRecebido_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Metodo_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelDinheiro == null || PanelPix == null || PanelCartao == null || BtnConfirmar == null) return;

            PararTimer();

            // Resetar visibilidades
            PanelDinheiro.Visibility = Visibility.Collapsed;
            PanelPix.Visibility = Visibility.Collapsed;
            PanelCartao.Visibility = Visibility.Collapsed;
            BtnConfirmar.IsEnabled = false;

            if (RadDinheiro.IsChecked == true)
            {
                MetodoSelecionado = "Dinheiro";
                PanelDinheiro.Visibility = Visibility.Visible;
                TxtValorRecebido.Focus();
                TxtValorRecebido.SelectAll();
                AtualizarTroco();
            }
            else if (RadPix.IsChecked == true)
            {
                MetodoSelecionado = "PIX";
                PanelPix.Visibility = Visibility.Visible;
                IniciarFluxoPix();
            }
            else if (RadCredito.IsChecked == true)
            {
                MetodoSelecionado = "Cartão de Crédito";
                PanelCartao.Visibility = Visibility.Visible;
            }
            else if (RadDebito.IsChecked == true)
            {
                MetodoSelecionado = "Cartão de Débito";
                PanelCartao.Visibility = Visibility.Visible;
            }
        }

        private void TxtValorRecebido_TextChanged(object sender, TextChangedEventArgs e)
        {
            AtualizarTroco();
        }

        private void AtualizarTroco()
        {
            if (TxtTroco == null || BtnConfirmar == null) return;

            string text = TxtValorRecebido.Text.Trim();
            if (decimal.TryParse(text, out decimal recebido))
            {
                ValorRecebido = recebido;
                if (recebido >= _total)
                {
                    Troco = recebido - _total;
                    TxtTroco.Text = Troco.ToString("C");
                    TxtTroco.Foreground = Brushes.LightGreen;
                    BtnConfirmar.IsEnabled = true;
                }
                else
                {
                    Troco = 0;
                    TxtTroco.Text = "Valor Insuficiente";
                    TxtTroco.Foreground = Brushes.Tomato;
                    BtnConfirmar.IsEnabled = false;
                }
            }
            else
            {
                Troco = 0;
                TxtTroco.Text = "R$ 0,00";
                TxtTroco.Foreground = Brushes.White;
                BtnConfirmar.IsEnabled = false;
            }
        }

        private async void IniciarFluxoPix()
        {
            TxtCarregandoPix.Visibility = Visibility.Visible;
            ImgQrCode.Source = null;
            TxtChavePix.Text = "Conectando ao Mercado Pago...";
            TxtStatusPix.Text = "Carregando...";
            TxtStatusPix.Foreground = Brushes.Gold;

            // Tenta criar o Pix via API do Mercado Pago
            var (qrCode, qrCodeBase64, paymentId, error) = await MercadoPagoService.CriarPagamentoPixAsync(_total);

            if (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(qrCodeBase64))
            {
                // PIX REAL ATIVO
                _paymentId = paymentId;
                _isRealPix = true;
                TxtChavePix.Text = qrCode;

                try
                {
                    byte[] binaryData = Convert.FromBase64String(qrCodeBase64);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.EndInit();
                    ImgQrCode.Source = bi;

                    TxtStatusPix.Text = "Aguardando pagamento automático...";
                    TxtStatusPix.Foreground = Brushes.Cyan;
                    _pixPollTimer.Start(); // Inicia a verificação de 3 em 3 segundos
                }
                catch (Exception ex)
                {
                    FallbackPixEstatico("Erro ao renderizar imagem do MP: " + ex.Message);
                }
            }
            else
            {
                // FALLBACK: Pix estático local
                FallbackPixEstatico(error ?? "Sem conexão");
            }

            TxtCarregandoPix.Visibility = Visibility.Collapsed;
        }

        private void FallbackPixEstatico(string razao)
        {
            _isRealPix = false;
            TxtStatusPix.Text = "Modo Estático: Confirme no seu app.";
            TxtStatusPix.Foreground = Brushes.Orange;

            // Exibe a razão do erro no box de chave como aviso
            string payload = PixHelper.GerarPayloadPix("edwilsondddss@gmail.com", _total);
            TxtChavePix.Text = payload;

            try
            {
                // Gera o QRCode estático localmente
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] pngBytes = qrCode.GetGraphic(20);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = new MemoryStream(pngBytes);
                    bitmap.EndInit();

                    ImgQrCode.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar QR Code local: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Habilita o botão de confirmação manual porque no modo estático não há consulta automática
            BtnConfirmar.IsEnabled = true;
        }

        private async void PixPollTimer_Tick(object sender, EventArgs e)
        {
            if (!_isRealPix || _paymentId == 0) return;

            var (status, error) = await MercadoPagoService.ConsultarStatusPagamentoAsync(_paymentId);

            if (string.IsNullOrEmpty(error))
            {
                if (status == "approved")
                {
                    PararTimer();
                    TxtStatusPix.Text = "✓ PAGO!";
                    TxtStatusPix.Foreground = Brushes.LightGreen;
                    
                    ProcessarSucessoPagamento();
                }
            }
        }

        private void PararTimer()
        {
            if (_pixPollTimer != null && _pixPollTimer.IsEnabled)
            {
                _pixPollTimer.Stop();
            }
        }

        private async void ProcessarSucessoPagamento()
        {
            PararTimer();
            OverlaySucesso.Visibility = Visibility.Visible;

            // Notifica o Central em segundo plano
            _ = System.Threading.Tasks.Task.Run(() => MercadoPagoService.NotificarVendaCentralAsync(_total, MetodoSelecionado, "approved"));

            await System.Threading.Tasks.Task.Delay(1800);
            DialogResult = true;
        }

        private void BtnConfirmarPix_Click(object sender, RoutedEventArgs e)
        {
            ProcessarSucessoPagamento();
        }

        private void BtnConfirmarCartao_Click(object sender, RoutedEventArgs e)
        {
            ProcessarSucessoPagamento();
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            ProcessarSucessoPagamento();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtValorRecebido_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtValorRecebido.SelectAll();
        }
    }
}
