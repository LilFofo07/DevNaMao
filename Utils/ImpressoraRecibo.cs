using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PDVModerno.Models;

namespace PDVModerno.Utils
{
    public static class ImpressoraRecibo
    {
        public static void ImprimirRecibo(IEnumerable<ItemVenda> carrinho, decimal total, string formaPagamento)
        {
            PrintDialog printDialog = new PrintDialog();

            // Mostra o diálogo de impressão para o usuário escolher a impressora
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = CriarDocumentoRecibo(carrinho, total, formaPagamento);
                
                // Formatar para a impressora selecionada (largura padrão de bobina térmica 80mm ~ 300px)
                doc.PageWidth = 300; 
                doc.PagePadding = new Thickness(10);
                doc.ColumnWidth = 300;

                // Margens e configurações de documento
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Recibo PDV");
            }
        }

        private static FlowDocument CriarDocumentoRecibo(IEnumerable<ItemVenda> carrinho, decimal total, string formaPagamento)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Courier New"); // Fonte monoespaçada padrão de recibo
            doc.FontSize = 12;

            // Cabeçalho
            Paragraph cabecalho = new Paragraph();
            cabecalho.TextAlignment = TextAlignment.Center;
            cabecalho.Inlines.Add(new Bold(new Run("MEU PDV\n")));
            cabecalho.Inlines.Add(new Run("Comprovante Não Fiscal\n"));
            cabecalho.Inlines.Add(new Run("--------------------------------\n"));
            cabecalho.Inlines.Add(new Run($"Data: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}\n"));
            cabecalho.Inlines.Add(new Run("--------------------------------\n"));
            doc.Blocks.Add(cabecalho);

            // Itens
            Paragraph itens = new Paragraph();
            itens.Margin = new Thickness(0);
            itens.Inlines.Add(new Bold(new Run("QTD   DESCRIÇÃO          V.TOTAL\n")));
            
            foreach (var item in carrinho)
            {
                // Formata os textos limitando tamanhos para alinhar
                string qtd = item.Quantidade.ToString().PadRight(5);
                string nome = item.Produto.Nome.Length > 15 ? item.Produto.Nome.Substring(0, 15) : item.Produto.Nome.PadRight(15);
                string preco = item.Subtotal.ToString("N2").PadLeft(10);
                
                itens.Inlines.Add(new Run($"{qtd} {nome} {preco}\n"));
                
                if (item.Desconto > 0)
                {
                    itens.Inlines.Add(new Run($"      Desconto:      -{item.Desconto.ToString("N2").PadLeft(10)}\n"));
                }
            }
            itens.Inlines.Add(new Run("--------------------------------\n"));
            doc.Blocks.Add(itens);

            // Total
            Paragraph rodape = new Paragraph();
            rodape.TextAlignment = TextAlignment.Right;
            rodape.Inlines.Add(new Bold(new Run($"TOTAL: R$ {total.ToString("N2")}\n")));
            rodape.Inlines.Add(new Run($"PAGAMENTO: {formaPagamento.ToUpper()}\n"));
            rodape.Inlines.Add(new Run("--------------------------------\n"));
            doc.Blocks.Add(rodape);

            // Mensagem Final
            Paragraph mensagemFinal = new Paragraph();
            mensagemFinal.TextAlignment = TextAlignment.Center;
            mensagemFinal.Inlines.Add(new Run("OBRIGADO PELA PREFERÊNCIA!\n"));
            mensagemFinal.Inlines.Add(new Run("Volte Sempre\n"));
            doc.Blocks.Add(mensagemFinal);

            return doc;
        }
    }
}
