using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueAvalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CatalogueAvalonia.Services.BillingService;

public class Invoice : ObservableRecipient
{
    private IMessenger _messenger;
    public Invoice(IMessenger messenger)
    {
        _messenger = messenger;
    }
    public Tuple<NotificationModel, Document> CreateDocument(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
        Settings.CheckIfAllTextGlyphsAreAvailable = false;
        notification.FileInfo = $"Накладная реализация за {mainGroup.Datetime} для {mainGroup.AgentName}";
        var doc = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.DefaultTextStyle(x => x.FontFamily("Arial"));
                page.Margin(15);
                page.Header().Height(2.5f, Unit.Centimetre)
                    .Element(Header);

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
                        row.AutoItem().AlignLeft().Text($"№{mainGroup.Id}").FontSize(10);
                        row.RelativeItem().AlignCenter().Text("Накладная Реализация").FontSize(15);
                    });
                }

                void HeaderRowLeft(IContainer container)
                {
                    container.Column(column =>
                        {
                            column.Item().Text($"Покупатель: {mainGroup.AgentName}")
                                .FontSize(10);
                            column.Item().Text($"Дата: {mainGroup.Datetime}").FontSize(10);
                            column.Item().Text($"Валюта: {mainGroup.CurrencyName}").FontSize(10);
                        }
                    );
                }

                void HeaderRowRight(IContainer container)
                {
                }


                page.Content().Element(Content);

                void Content(IContainer container)
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
                                .AlignCenter().Text($"Цена({mainGroup.CurrencySign})")
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
                                .AlignCenter().Text($"Сумма({mainGroup.CurrencySign})")
                                .FontFamily("Arial")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                        });

                        var count = 1;
                        var totalCount = 0;
                        decimal totalSumLast = 0;
                        notification.TotalSteps = parts.Count() + 1;
                        foreach (var part in parts)
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
                            notification.CurrStep = count -1;
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


                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontFamily("Arial")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1)
                    );

                    text.Span("Страница ");

                    text.CurrentPageNumber();

                    text.Span(" из ");

                    text.TotalPages();
                });
            });

        });
        string path = $"../Documents/{fileId}Invoice{mainGroup.Id}от{mainGroup.Datetime}.pdf";
        var f = Directory.GetCurrentDirectory();
        notification.FilePath = f.Substring(0, f.LastIndexOf('\\')) + path.TrimStart('.').Replace('/', '\\');
        notification.StatusOfFile = FileStatus.Ready;
        notification.Description = new DescriptionModel
        {
            StartDate = mainGroup.Datetime,
            Description = "Накладная реализация\n" +
                          $"Номер файла: {fileId}\n" +
                          $"Дата: {mainGroup.Datetime}\n" +
                          $"Путь к файлу: {notification.FilePath}"
        };
        return new Tuple<NotificationModel, Document>(notification, doc);
    }
}