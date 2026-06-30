using System;
using System.Text;

namespace PDVModerno.Utils
{
    public static class PixHelper
    {
        public static string GerarPayloadPix(string chave, decimal valor, string nomeRecebedor = "MEU PDV", string cidadeRecebedor = "SAO PAULO", string txId = "PDVMODERNO")
        {
            // 00 (Payload Format Indicator): sempre "01"
            string payloadFormat = "000201";

            // 26 (Merchant Account Information)
            // - 00 (GUI): "br.gov.bcb.pix"
            // - 01 (Chave Pix): e-mail, telefone, CPF etc.
            string gui = "0014br.gov.bcb.pix";
            string key = "01" + chave.Length.ToString("D2") + chave;
            string merchantAccount = "26" + (gui.Length + key.Length).ToString("D2") + gui + key;

            // 52 (Merchant Category Code): "0000" para genérico
            string categoryCode = "52040000";

            // 53 (Transaction Currency): "986" (BRL - Real)
            string currency = "5303986";

            // 54 (Transaction Amount): formatado com 2 casas decimais e ponto
            string valStr = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            string amount = "54" + valStr.Length.ToString("D2") + valStr;

            // 58 (Country Code): "BR"
            string country = "5802BR";

            // 59 (Merchant Name): nome do recebedor
            string merchantName = "59" + nomeRecebedor.Length.ToString("D2") + nomeRecebedor;

            // 60 (Merchant City): cidade do recebedor
            string merchantCity = "60" + cidadeRecebedor.Length.ToString("D2") + cidadeRecebedor;

            // 62 (Additional Data Field Template)
            // - 05 (Reference Label / TxId): Identificação da transação
            string refLabel = "05" + txId.Length.ToString("D2") + txId;
            string additionalData = "62" + refLabel.Length.ToString("D2") + refLabel;

            // Une todos os blocos até o identificador de CRC16
            string partialPayload = payloadFormat + merchantAccount + categoryCode + currency + amount + country + merchantName + merchantCity + additionalData;
            partialPayload += "6304"; // ID 63, tamanho 04

            // Calcula o CRC16 e adiciona no fim em hexadecimal maiúsculo
            ushort crc = CalcularCRC16(partialPayload);
            string crcHex = crc.ToString("X4");

            return partialPayload + crcHex;
        }

        private static ushort CalcularCRC16(string data)
        {
            ushort crc = 0xFFFF;
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return (ushort)(crc & 0xFFFF);
        }
    }
}
