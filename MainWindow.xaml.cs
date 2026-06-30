using System.Windows;
using PDVModerno.Views;

namespace PDVModerno
{
    public partial class MainWindow : Window
    {
        private PDVView pdvView;
        private ProdutosView produtosView;
        private EstoqueView estoqueView;
        private SuporteView suporteView;

        public MainWindow()
        {
            InitializeComponent();
            
            // Instanciar as views
            pdvView = new PDVView();
            produtosView = new ProdutosView();
            estoqueView = new EstoqueView();
            suporteView = new SuporteView();
            
            // Carregar view inicial
            MainContent.Content = pdvView;

            // Escutar teclado globalmente
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            // Iniciar Relógio
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = System.TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { TxtRelogio.Text = System.DateTime.Now.ToString("HH:mm:ss"); };
            timer.Start();
            TxtRelogio.Text = System.DateTime.Now.ToString("HH:mm:ss"); // Forçar atualização imediata
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.F1:
                    pdvView?.ProcessarAtalhoF1();
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.F2:
                    pdvView?.ProcessarAtalhoF2();
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.F3:
                    NavProdutos.IsChecked = true;
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.F4:
                    this.WindowState = WindowState.Minimized;
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.F5:
                    pdvView?.AlternarCaixa();
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Space:
                    // Apenas finaliza se a tela atual for o PDV e se o usuário não estiver digitando em uma caixa de texto que aceita espaços (ex: InputDialog)
                    if (MainContent.Content == pdvView && !(System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox tb && !tb.IsReadOnly && tb.Name != "TxtLeitorCodigo"))
                    {
                        pdvView?.ProcessarAtalhoEspaco();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void NavPdv_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent != null && pdvView != null)
                MainContent.Content = pdvView;
        }

        private void NavProdutos_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent != null && produtosView != null)
                MainContent.Content = produtosView;
        }

        private void NavEstoque_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent != null && estoqueView != null)
                MainContent.Content = estoqueView;
        }

        private void NavSuporte_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent != null && suporteView != null)
                MainContent.Content = suporteView;
        }

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}