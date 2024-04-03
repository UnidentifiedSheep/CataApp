using System;
using System.Collections.Generic;
using System.IO;
using CatalogueAvalonia.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace CatalogueAvalonia.Services.BillingService;

public static class Invoice 
{
    
    public static void CreateInvoice(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup)
    {
		CreateDocument(parts, mainGroup);
    }

    private static void CreateDocument(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup)
    { 
	    Document.Create(document => 
	    { 
		    document.Page(page => 
		    { 
			    page.DefaultTextStyle(x => x.FontFamily("Arial")); 
			    page.Margin(15); 
			    page.Header().Height(4, Unit.Centimetre)
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
					    row.AutoItem().AlignLeft().Text($"№{mainGroup.Id}").FontSize(12); 
					    row.RelativeItem().AlignCenter().Text("Накладная Реализация").FontSize(15); 
				    }); 
			    }
			    
			    void HeaderRowLeft(IContainer container) 
			    { 
				    container.Column(column => 
					    { 
						    column.Item().Text($"Покупатель: {mainGroup.AgentName}")
							    .FontSize(14); 
						    column.Item().Text($"Дата: {mainGroup.Datetime}").FontSize(14); 
						    column.Item().Text($"Валюта: {mainGroup.CurrencyName}").FontSize(14); 
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
							    .MinHeight(20)
							    .AlignMiddle()
							    .AlignCenter().Text("#")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black);
						    
						    header.Cell().Border(1)
							    .Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .PaddingLeft(5)
							    .AlignLeft().Text("Номер запчасти")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
						    header.Cell().Border(1).Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .PaddingLeft(5)
							    .AlignLeft().Text("Название запчасти")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
						    header.Cell().Border(1)
							    .Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .PaddingLeft(5)
							    .AlignCenter().Text("Производитель")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
						    header.Cell().Border(1)
							    .Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .AlignCenter().Text($"Цена({mainGroup.CurrencySign})")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
						    header.Cell().Border(1)
							    .Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .AlignCenter().Text("Кол-во")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
						    header.Cell().Border(1)
							    .Background(Colors.Grey.Lighten2)
							    .MinHeight(20)
							    .AlignMiddle()
							    .AlignCenter().Text($"Сумма({mainGroup.CurrencySign})")
							    .FontFamily("Arial")
							    .FontSize(12)
							    .FontColor(Colors.Black); 
					    });

					    int count = 1;
					    int totalCount = 0;
					    double totalSumLast = 0;
					    foreach (var part in parts)
					    {
						    totalCount += part.Count;
						    totalSumLast += part.PriceSum;
						    
						    table.Cell().Border(1)
							    .AlignMiddle()
							    .AlignCenter()
							    .Text(count.ToString()); 
						    table.Cell().Border(1)
							    .PaddingLeft(5)
							    .AlignLeft()
							    .AlignMiddle()
							    .Text(part.UniValue); 
						    table.Cell().Border(1)
							    .PaddingLeft(5)
							    .AlignLeft()
							    .AlignMiddle()
							    .Text(part.MainCatName); 
						    table.Cell().Border(1)
							    .AlignCenter()
							    .AlignMiddle()
							    .Text(part.ProducerName); 
						    table.Cell().Border(1)
							    .AlignMiddle()
							    .AlignCenter()
							    .Text($"{part.Price:F}"); 
						    table.Cell().Border(1)
							    .AlignCenter()
							    .AlignMiddle()
							    .Text(part.Count.ToString()); 
						    table.Cell().Border(1)
							    .AlignCenter()
							    .AlignMiddle()
							    .Text($"{part.PriceSum:F}");
						    
						    count++;
					    }


					    
					    table.Cell().Border(1)
						    .AlignMiddle()
						    .AlignCenter()
						    .Text("Итого"); 
					    table.Cell()
						    .ColumnSpan(4)
						    .Border(1)
						    .PaddingLeft(5)
						    .AlignLeft()
						    .AlignMiddle()
						    .Text("");
					    
					    table.Cell().Border(1)
						    .AlignCenter()
						    .AlignMiddle()
						    .Text(totalCount.ToString()); 
					    table.Cell().Border(1)
						    .AlignCenter()
						    .AlignMiddle()
						    .Text($"{totalSumLast:F}"); 
				    }); 
			    }

			    
			    page.Footer().AlignCenter().Text(text => 
			    { 
				    text.DefaultTextStyle(x => x.Fallback
					    (
						    z => z.FontFamily("Arial"))
					    .FontSize(15)
					    .FontColor(Colors.Grey.Darken1)
				    );
				    
				    text.Span("Страница ");
				    
				    text.CurrentPageNumber();
				    
				    text.Span(" из ");
				    
				    text.TotalPages(); 
			    }); 
		    }); 
	    }).GeneratePdfAndShow();
	    //GeneratePdf($"../Documents/Invoice{DateTime.Now.ToString("dd/MM/yyyy")}.pdf");
    }
}