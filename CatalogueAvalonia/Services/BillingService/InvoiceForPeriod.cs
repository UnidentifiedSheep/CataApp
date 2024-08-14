using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.BillingService.Components;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace CatalogueAvalonia.Services.BillingService;

public class InvoiceForPeriod
{
    private readonly IMessenger _messenger;
    public InvoiceForPeriod(IMessenger messenger)
    {
        _messenger = messenger;
    }
    public Tuple<NotificationModel, Document> CreateInvoice(IEnumerable<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>> tuples, DateTime start, DateTime end, int fileId, NotificationModel notification)
    {
        Settings.CheckIfAllTextGlyphsAreAvailable = false;
        notification.FileInfo = $"Накладные реализации зa {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
        
        var doc = Document.Create(document =>
        {
            decimal totalSumS = 0;
            int countS = 0;
            var countSteps = 0;
            notification.TotalSteps = tuples.Count() +1;
            foreach (var item in tuples)
            {
                totalSumS += item.Item1.TotalSum;
                countS += item.Item2.Sum(x => x.Count ?? 0);

                document.Page(page =>
                {
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));
                    page.Margin(15);
                    page.Header().Height(2.5f, Unit.Centimetre)
                        .Element(Header);

                    page.Content().Element(Content);

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x =>
                            x.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken1));

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
                        row.AutoItem().AlignLeft().Text($"№{item.Item1.Id}").FontSize(10);
                        row.RelativeItem().AlignCenter().Text("Накладная Реализация").FontSize(15);
                    });
                }

                void HeaderRowLeft(IContainer container)
                {
                    container.Column(column =>
                        {
                            column.Item().Text($"Покупатель: {item.Item1.AgentName}")
                                .FontSize(14);
                            column.Item().Text($"Дата: {item.Item1.Datetime}").FontSize(10);
                            column.Item().Text($"Валюта: {item.Item1.CurrencyName}").FontSize(10);
                        }
                    );
                }

                void HeaderRowRight(IContainer container)
                {
                    container.Column(column =>
                    {
                        column.Item().Text($"Детали/Комментарий: {item.Item1.Comment}").FontSize(10);
                    });
                }

                void Content(IContainer container)
                {
                    container.Column(col =>
                    {
                        col.Item().Element(ContentTop);
                        col.Item().Dynamic(new LastPageTotalSum(countS, totalSumS));
                    });
                }

                void ContentTop(IContainer container)
                {
                    container.PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            //#, UniValue, MainCatName, Producer, Price, Count, TotalPriceSum
                            columns.RelativeColumn(1.3f);
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
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .PaddingLeft(5)
                                .AlignLeft().Text("Номер запчасти")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .PaddingLeft(5)
                                .AlignLeft().Text("Название запчасти")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .PaddingLeft(5)
                                .AlignCenter().Text("Производитель")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .AlignCenter().Text($"Цена({item.Item1.CurrencySign})")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .AlignCenter().Text("Кол-во")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                            header.Cell().Border(1)
                                .Background(Colors.Grey.Lighten2)
                                .AlignMiddle()
                                .AlignCenter().Text($"Сумма({item.Item1.CurrencySign})")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                        });

                        var count = 1;
                        var totalCount = 0;
                        decimal totalSumLast = 0;
                        foreach (var part in item.Item2)
                        {
                            totalCount += part.Count ?? 0;
                            totalSumLast += part.PriceSum;

                            table.Cell().Border(1)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text(count.ToString())
                                .FontSize(8);
                            table.Cell().Border(1)
                                .PaddingLeft(5)
                                .AlignLeft()
                                .AlignMiddle()
                                .Text(part.UniValue)
                                .FontSize(8);
                            var name = part.MainCatName;
                            if (string.IsNullOrWhiteSpace(part.MainCatName) || part.MainCatName == "Название не указано")
                                name = part.MainName;
                            table.Cell().Border(1)
                                .PaddingLeft(5)
                                .AlignLeft()
                                .AlignMiddle()
                                .Text(name)
                                .ClampLines(1, "...")
                                .FontSize(8);
                            table.Cell().Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(part.ProducerName)
                                .FontSize(8);
                            table.Cell().Border(1)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text($"{part.Price:F}")
                                .FontSize(8);
                            table.Cell().Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(part.Count.ToString())
                                .FontSize(8);
                            table.Cell().Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text($"{part.PriceSum:F}")
                                .FontSize(8);

                            count++;
                        }


                        table.Cell().Border(1)
                            .AlignMiddle()
                            .AlignCenter()
                            .Text("Итого")
                            .FontSize(8);
                        table.Cell()
                            .ColumnSpan(4)
                            .Border(1)
                            .PaddingLeft(5)
                            .AlignLeft()
                            .AlignMiddle()
                            .Text("")
                            .FontSize(8);

                        table.Cell().Border(1)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(totalCount.ToString())
                            .FontSize(8);
                        table.Cell().Border(1)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{totalSumLast:F}")
                            .FontSize(8);
                    });

                }

                countSteps++;
                notification.CurrStep = countSteps;
            }
        });
        string path = $"../Documents/{fileId}InvoiceОт{start:dd/MM/yyyy}-{end:dd/MM/yyyy}.pdf";
        notification.FilePath = Directory.GetCurrentDirectory().Replace("\\bin", "").Replace("\\net8.0", "") + path.TrimStart('.').Replace('/', '\\');
        notification.Description = new DescriptionModel
        {
            StartDate = start.ToString("dd/MM/yyyy"),
            EndDate = end.ToString("dd/MM/yyyy"),
            Description = "Накладная реализация\n" +
                          $"Номер файла: {fileId}\n" +
                          $"Дата: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}\n" +
                          $"Путь к файлу: {notification.FilePath}"
        };
        return new Tuple<NotificationModel, Document>(notification, doc);
    }
}