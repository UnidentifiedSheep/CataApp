using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using OfficeOpenXml;
using OfficeOpenXml.Style;


namespace CatalogueAvalonia.Services.BillingService;

public class ExcelInvoice
{
    private readonly IMessenger _messenger;
    public ExcelInvoice(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public async Task CreateExcel(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
        string path = $"../Documents/{fileId}ExcelInvoice{mainGroup.Id}от{mainGroup.Datetime}.xlsx";
        var f = Directory.GetCurrentDirectory();
        notification.FilePath = f.Substring(0, f.LastIndexOf('\\')) + path.TrimStart('.').Replace('/', '\\');
        using (var excelPackage = new ExcelPackage(notification.FilePath))
        {
            var sh = excelPackage.Workbook.Worksheets.Add("Main");
            
            notification.FileInfo = $"Execel Накладная реализация за {mainGroup.Datetime} для {mainGroup.AgentName}";
            var prodajaAltModels = parts.ToList();
            notification.TotalSteps = prodajaAltModels.Count + 1;
            
            sh.Cells["A1"].Value = "Накладная реализации";
            sh.Cells["A2"].Value = "Номер накладной:";
            sh.Cells["B2"].Value = mainGroup.Id;
            sh.Cells["A3"].Value = "Дата:";
            sh.Cells["B3"].Value = $"{mainGroup.Datetime}";
            sh.Cells["A4"].Value = $"Покупатель:";
            sh.Cells["B4"].Value = $"{mainGroup.AgentName}";
            sh.Cells["A5"].Value = "Валюта:";
            sh.Cells["B5"].Value = $"{mainGroup.CurrencyName}";
            notification.CurrStep = 1;
            
            sh.Cells["A7"].Value = "Номер запчасти";
            sh.Cells["B7"].Value = "Наименование";
            sh.Cells["C7"].Value = "Производитель";
            sh.Cells["D7"].Value = $"Цена({mainGroup.CurrencySign})";
            sh.Cells["E7"].Value = "Количество";
            sh.Cells["F7"].Value = $"Сумма{mainGroup.CurrencySign}";
            sh.Cells["G7"].Value = $"Комментарий/номер машины";
            for (int i = 1; i < 7; i++)
                sh.Cells[7, i].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            sh.Cells["A7:G7"].Style.Font.Bold = true;

            int row = 8;
            foreach (var part in prodajaAltModels)
            {
                var name = part.MainCatName;
                if (string.IsNullOrWhiteSpace(part.MainCatName) || part.MainCatName == "Название не указано")
                    name = part.MainName;
                sh.Cells[$"A{row}"].Value = part.UniValue;
                sh.Cells[$"B{row}"].Value = name;
                sh.Cells[$"C{row}"].Value = part.ProducerName;
                sh.Cells[$"D{row}"].Value = part.Price;
                sh.Cells[$"E{row}"].Value = part.Count;
                sh.Cells[$"F{row}"].Value = part.PriceSum;
                sh.Cells[$"G{row}"].Value = part.Comment != null ? part.Comment.TrimStart(' ').TrimEnd(' ') : "";
                
                for (int i = 1; i < 8; i++)
                    sh.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                notification.CurrStep++;
                row++;
            }
            sh.Cells[$"E{row}"].Value = "Итого:";
            sh.Cells[$"F{row}"].Value = prodajaAltModels.Sum(x => x.PriceSum);
            
            for (int i = 1; i < 7; i++)
                sh.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            sh.Cells[$"A1:G{row}"].AutoFitColumns();
            
            await excelPackage.SaveAsync();
            GC.Collect();
            notification.StatusOfFile = FileStatus.Ready;
            notification.Description = new DescriptionModel
        {
            StartDate = mainGroup.Datetime,
            Description = "Накладная реализация\n" +
                          $"Номер файла: {fileId}\n" +
                          $"Дата: {mainGroup.Datetime}\n" +
                          $"Путь к файлу: {notification.FilePath}"
        };
        }
        
        
        _messenger.Send(new EditedMessage(new ChangedItem
            { Id = notification.FileId, MainName = "", Where = "FileReady" }));
    }
}