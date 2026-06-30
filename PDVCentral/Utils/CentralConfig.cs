using System;
using System.IO;
using System.Text.Json;

namespace PDVCentral.Utils
{
    public static class CentralConfig
    {
        public static string ApiUrl { get; private set; } = "http://localhost:5080/";

        static CentralConfig()
        {
            CarregarConfig();
        }

        public static void CarregarConfig()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "central_config.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("ApiUrl", out JsonElement urlElem))
                    {
                        ApiUrl = urlElem.GetString() ?? "http://localhost:5080/";
                    }
                }
                else
                {
                    // Cria arquivo de configuração padrão
                    var payload = new { ApiUrl = "http://localhost:5080/" };
                    File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch
            {
                // Ignora falhas silenciosamente
            }
        }
    }
}
