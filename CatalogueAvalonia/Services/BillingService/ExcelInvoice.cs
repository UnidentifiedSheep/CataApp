using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;
using ExcelLibrary.SpreadSheet;


namespace CatalogueAvalonia.Services.BillingService;

public class ExcelInvoice
{
    private readonly IMessenger _messenger;
    public ExcelInvoice(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public Workbook CreateExcel(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
        var prodajaAltModels = parts.ToList();
        
        notification.FileInfo = $"Execel Накладная реализация за {mainGroup.Datetime} для {mainGroup.AgentName}";
        Workbook wb = new Workbook();
        Worksheet sh = new Worksheet("Main");
        
        notification.TotalSteps = prodajaAltModels.Count + 1;
        sh.Cells[0, 0] = new Cell("Накладная реализации");
        sh.Cells[1, 0] = new Cell("Номер накладной:");
        sh.Cells[1, 1] = new Cell($"{mainGroup.Id}");
        sh.Cells[2, 0] = new Cell("Дата:");
        sh.Cells[2, 1] = new Cell($"{mainGroup.Datetime}");
        sh.Cells[3, 0] = new Cell($"Покупатель:");
        sh.Cells[3, 1] = new Cell($"{mainGroup.AgentName}");
        sh.Cells[4, 0] = new Cell("Валюта:");
        sh.Cells[4, 1] = new Cell($"{mainGroup.CurrencyName}");
        notification.CurrStep = 1;
        sh.Cells[6, 0] = new Cell("Номер запчасти");
        sh.Cells[6, 1] = new Cell("Наименование");
        sh.Cells[6, 2] = new Cell("Производитель");
        sh.Cells[6, 3] = new Cell($"Цена({mainGroup.CurrencySign})");
        sh.Cells[6, 4] = new Cell("Количество");
        sh.Cells[6, 5] = new Cell("Сумма");

        int row = 7;
        foreach (var part in prodajaAltModels)
        {
            sh.Cells[row, 0] = new Cell(part.UniValue);
            sh.Cells[row, 1] = new Cell(part.MainCatName);
            sh.Cells[row, 2] = new Cell(part.ProducerName);
            sh.Cells[row, 3] = new Cell(part.Price);
            sh.Cells[row, 4] = new Cell(part.Count);
            sh.Cells[row, 5] = new Cell(part.PriceSum);
            notification.CurrStep++;
            row++;
        }
        sh.Cells[row, 4] = new Cell("Итого:");
        sh.Cells[row, 5] = new Cell($"{prodajaAltModels.Sum(x => x.PriceSum)}{mainGroup.CurrencySign}");
        
        string path = $"../Documents/{fileId}ExcelInvoice{mainGroup.Id}от{mainGroup.Datetime}.xlsx";
        notification.FilePath = Directory.GetCurrentDirectory().Replace("\\bin", "").Replace("\\net8.0", "") + path.TrimStart('.').Replace('/', '\\');
        wb.Worksheets.Add(sh);
        wb.Save(path);
        notification.StatusOfFile = FileStatus.Ready;
        notification.Description = new DescriptionModel
        {
            StartDate = mainGroup.Datetime,
            Description = "Накладная реализация\n" +
                          $"Номер файла: {fileId}\n" +
                          $"Дата: {mainGroup.Datetime}\n" +
                          $"Путь к файлу: {notification.FilePath}"
        };
        _messenger.Send(new EditedMessage(new ChangedItem
            { Id = notification.FileId, MainName = "", Where = "FileReady" }));
        return wb;
    }
}