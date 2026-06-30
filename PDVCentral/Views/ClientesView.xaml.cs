using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using PDVCentral.Models;
using PDVCentral.Utils;

namespace PDVCentral.Views
{
    public partial class ClientesView : UserControl
    {
        private static readonly HttpClient _httpClient = new();

        public ClientesView()
        {
            InitializeComponent();
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
                    var lista = JsonSerializer.Deserialize<List<Cliente>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();

                    string filtro = TxtFiltro.Text.Trim().ToLower();
                    var filtrada = lista.AsEnumerable();

                    if (!string.IsNullOrEmpty(filtro))
                    {
                        filtrada = filtrada.Where(c => c.Nome.ToLower().Contains(filtro) || c.Cnpj.Contains(filtro));
                    }

                    GridClientes.ItemsSource = filtrada.OrderBy(c => c.Nome).ToList();
                }
                else
                {
                    MessageBox.Show($"Falha ao carregar clientes da API: {(int)response.StatusCode}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão com a API Central: {ex.Message}", "Erro de Rede", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void BtnNovoCliente_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientEditDialog(null)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Cliente cliente)
            {
                var dialog = new ClientEditDialog(cliente)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private async void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Cliente cliente)
            {
                try
                {
                    cliente.Status = cliente.Status == "Ativo" ? "Bloqueado" : "Ativo";
                    
                    string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes";
                    string json = JsonSerializer.Serialize(cliente);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show($"Falha ao atualizar status: HTTP {(int)response.StatusCode}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de conexão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Cliente cliente)
            {
                var result = MessageBox.Show($"Deseja realmente excluir o cliente '{cliente.Nome}'?", "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string url = $"{CentralConfig.ApiUrl.TrimEnd('/')}/api/clientes/{cliente.Cnpj}";
                        HttpResponseMessage response = await _httpClient.DeleteAsync(url);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show($"Falha ao excluir cliente: HTTP {(int)response.StatusCode}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro de conexão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
