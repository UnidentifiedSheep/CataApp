using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace CatalogueAvalonia.Services.BillingService;

public class InvoiceForPeriodMinimal
{
    private readonly IMessenger _messenger;
    public InvoiceForPeriodMinimal(IMessenger messenger)
    {
        _messenger = messenger;
    }
    public Tuple<NotificationModel, Document> CreateInvoice(IEnumerable<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>> tuples, DateTime startDate, DateTime endDate, int fileId, NotificationModel notification)
    {
        Settings.CheckIfAllTextGlyphsAreAvailable = false;
        var agent = tuples.First();
        bool isAllAgentsSame = tuples.All(x => x.Item1.AgentId == agent.Item1.AgentId);
        bool isAllDetailsSame = tuples.All(x => x.Item1.Comment == agent.Item1.Comment);
        bool isAllCurrencySame = tuples.All(x => x.Item1.CurrencyId == agent.Item1.CurrencyId);
        if (isAllAgentsSame)
            notification.FileInfo = $"Накладные реализации зa {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}" +
                                    $"для {agent.Item1.AgentName}";
        else
            notification.FileInfo = $"Накладные реализации зa {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";

        var doc = Document.Create(doc =>
        {
            decimal totalSumS = 0;
            int countS = 0;

            doc.Page(page =>
            {
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
                page.Margin(15);
                page.Header().Height(2.8f, Unit.Centimetre)
                    .Element(Header);

                page.Content().Element(Content);

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.Fallback
                        (
                            z => z.FontFamily("Arial"))
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken1)
                    );

                    text.Span("Страница ");

                    text.CurrentPageNumber();

                    text.Span(" из ");

                    text.TotalPages();
                });
            });


            void Header(IContainer container)
            {
                container.Column(col =>
                {
                    col.Item().Element(HeaderTop);
                    col.Spacing(10);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(HeaderRowLeft);
                        row.RelativeItem().Element(HeaderRowRight);
                    });
                });
            }

            void HeaderTop(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().AlignCenter().Text("Накладная Реализация за период").FontSize(10);
                });
            }

            void HeaderRowLeft(IContainer container)
            {
                container.Column(column =>
                    {
                        if (isAllAgentsSame)
                            column.Item().Text($"Покупатель: {agent.Item1.AgentName}").FontSize(10);

                        column.Item().Text($"Дата: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}").FontSize(10);
                        if (isAllCurrencySame)
                            column.Item().Text($"Валюта: {agent.Item1.CurrencyName}").FontSize(10);

                    }
                );
            }

            void HeaderRowRight(IContainer container)
            {
                container.Column(column =>
                {
                    if (isAllDetailsSame)
                        column.Item().Text($"Детали/Комментарий: {agent.Item1.Comment}").FontSize(10);

                });
            }

            void Content(IContainer container)
            {
                container.PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        //#, AgentName, Comment, UniValue, MainCatName, Producer, Price, Count, TotalPriceSum
                        columns.RelativeColumn(1.3f);
                        if (!isAllAgentsSame)
                            columns.RelativeColumn(4);
                        if (!isAllDetailsSame)
                            columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(3.2f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);

                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1)
                            .Background(Colors.Grey.Lighten2)
                            .AlignMiddle()
                            .AlignCenter().Text("#")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);

                        if (!isAllAgentsSame)
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .PaddingLeft(5)
                                .AlignLeft().Text("Контрагент")
                                .FontFamily("Arial")
                                .FontSize(9)
                                .FontColor(Colors.Black);
                        if (!isAllDetailsSame)
                            header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .PaddingLeft(5)
                                .AlignLeft().Text("Комментарий")
                                .FontFamily("Arial")
                                .FontSize(9)
                                .FontColor(Colors.Black);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                            .AlignMiddle()
                            .PaddingLeft(5)
                            .AlignLeft().Text("Номер запчасти")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                            .AlignMiddle()
                            .PaddingLeft(5)
                            .AlignLeft().Text("Название запчасти")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);
                        header.Cell().Border(1)
                            .Background(Colors.Grey.Lighten2)
                            .MinHeight(20)
                            .AlignMiddle()
                            .PaddingLeft(5)
                            .AlignCenter().Text("Производитель")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);
                        if (isAllCurrencySame)
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .MinHeight(20)
                                .AlignMiddle()
                                .AlignCenter().Text($"Цена({agent.Item1.CurrencySign})")
                                .FontFamily("Arial")
                                .FontSize(9)
                                .FontColor(Colors.Black);
                        else
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .MinHeight(20)
                                .AlignMiddle()
                                .AlignCenter().Text($"Цена")
                                .FontFamily("Arial")
                                .FontSize(9)
                                .FontColor(Colors.Black);

                        header.Cell().Border(1)
                            .Background(Colors.Grey.Lighten2)
                            .MinHeight(20)
                            .AlignMiddle()
                            .AlignCenter().Text("Кол-во")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);
                        header.Cell().Border(1)
                            .Background(Colors.Grey.Lighten2)
                            .MinHeight(20)
                            .AlignMiddle()
                            .AlignCenter().Text($"Сумма({agent.Item1.CurrencySign})")
                            .FontFamily("Arial")
                            .FontSize(9)
                            .FontColor(Colors.Black);
                    });

                    var count = 1;
                    var countStep = 0;
                    var totals = new List<Totals>();
                    notification.TotalSteps = tuples.Count() + 1;
                    foreach (var level0 in tuples)
                    {
                        foreach (var level1 in level0.Item2)
                        {
                            var total = totals.FirstOrDefault(x => x.CurrencyId == level0.Item1.CurrencyId);
                            if (total == null)
                                totals.Add(new Totals
                                {
                                    CurrencyId = level0.Item1.CurrencyId,
                                    TotalCount = level1.Count ?? 0,
                                    TotalSum = level1.PriceSum,
                                    CurrencyName = level0.Item1.CurrencyName,
                                    CurrencySign = level0.Item1.CurrencySign ?? "Un"
                                });
                            else
                            {
                                total.TotalSum += level1.PriceSum;
                                total.TotalCount += level1.Count ?? 0;
                            }

                            //Номер строки
                            table.Cell().Border(1)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text(count.ToString())
                                .FontSize(9);
                            //Имя контрагента
                            if (!isAllAgentsSame)
                                table.Cell().Border(1)
                                    .PaddingLeft(5)
                                    .AlignLeft()
                                    .AlignMiddle()
                                    .Text(level0.Item1.AgentName)
                                    .FontSize(9);
                            if (!isAllDetailsSame)
                                table.Cell().Border(1)
                                    .PaddingLeft(5)
                                    .AlignLeft()
                                    .AlignMiddle()
                                    .Text(level0.Item1.Comment ?? " ")
                                    .FontSize(9);
                            table.Cell().Border(1)
                                .PaddingLeft(5)
                                .AlignLeft()
                                .AlignMiddle()
                                .Text(level1.UniValue)
                                .FontSize(9);
                            var name = level1.MainCatName;
                            if (string.IsNullOrWhiteSpace(level1.MainCatName) || level1.MainCatName == "Название не указано")
                                name = level1.MainName;
                            table.Cell().Border(1)
                                .PaddingLeft(5)
                                .AlignLeft()
                                .AlignMiddle()
                                .Text(name)
                                .ClampLines(1, "...")
                                .FontSize(9);
                            table.Cell().Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(level1.ProducerName)
                                .FontSize(9);
                            table.Cell().Border(1)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text($"{level1.Price:F}")
                                .FontSize(9);
                            table.Cell().Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(level1.Count.ToString())
                                .FontSize(9);
                            if (isAllCurrencySame)
                                table.Cell().Border(1)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text($"{level1.PriceSum:F}")
                                    .FontSize(9);
                            else
                                table.Cell().Border(1)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text($"{level1.PriceSum:F}{level0.Item1.CurrencySign ?? "Un"}")
                                    .FontSize(9);

                            count++;
                        }

                        countStep++;
                        notification.CurrStep = countStep;
                    }

                    uint tf = Convert.ToUInt32(isAllDetailsSame) + Convert.ToUInt32(isAllAgentsSame);
                    uint countSpan = 5 - tf;
                    foreach (var item in totals)
                    {
                        table.Cell()
                            .Border(1)
                            .AlignMiddle()
                            .AlignCenter()
                            .Text($"Итого")
                            .FontSize(10);
                        table.Cell()
                            .Border(1)
                            .AlignMiddle()
                            .AlignCenter()
                            .Text($"{item.CurrencyName}")
                            .FontSize(10);
                        table.Cell()
                            .ColumnSpan(countSpan)
                            .Border(1)
                            .PaddingLeft(5)
                            .AlignLeft()
                            .AlignMiddle()
                            .Text("")
                            .FontSize(10);

                        table.Cell().Border(1)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(item.TotalCount.ToString())
                            .FontSize(10);
                        table.Cell().Border(1)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{item.TotalSum:F}{item.CurrencySign}")
                            .FontSize(10);
                    }
                });
            }
        });
        string path = $"../Documents/{fileId}InvoiceОт{startDate:dd/MM/yyyy}-{endDate:dd/MM/yyyy}.pdf";
        notification.CurrStep++;
        notification.FilePath = Directory.GetCurrentDirectory().Replace("\\bin", "").Replace("\\net8.0", "") + path.TrimStart('.').Replace('/', '\\');
        notification.Description = new DescriptionModel
        {
            StartDate = startDate.ToString("dd/MM/yyyy"),
            EndDate = endDate.ToString("dd/MM/yyyy"),
            Description = "Накладная реализация\n" +
                          $"Номер файла: {fileId}\n" +
                          $"Дата: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\n" +
                          $"Путь к файлу: {notification.FilePath}"
        };
        return new Tuple<NotificationModel, Document>(notification, doc);
    }
}

public class Totals
{
    public int CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
    public string CurrencySign { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public decimal TotalSum { get; set; }
}