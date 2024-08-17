using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using QuestPDF.Fluent;

namespace CatalogueAvalonia.Services.BillingService;

public class MainInvoice : ObservableRecipient
{
    private readonly Invoice _invoice;
    private readonly InvoiceForPeriodMinimal _invoiceForPeriodMinimal;
    private readonly InvoiceForPeriod _invoiceForPeriod;
    private readonly ExcelInvoice _excelInvoice;
    private readonly List<Tuple<NotificationModel, Document>> _generatorQueu = new ();

    public MainInvoice(Invoice invoice, InvoiceForPeriodMinimal invoiceForPeriodMinimal,
        InvoiceForPeriod invoiceForPeriod, ExcelInvoice excelInvoice, IMessenger messenger) : base(messenger)
    {
        _invoice = invoice;
        _invoiceForPeriod = invoiceForPeriod;
        _invoiceForPeriodMinimal = invoiceForPeriodMinimal;
        _excelInvoice = excelInvoice;
    }

    private void TryUnqueu()
    {
        if (_isUnqueuing)
        {
            GC.Collect();
            return;
        }
        Task.Run(UnQueu);
    }

    private bool _isUnqueuing = false;
    private async Task UnQueu()
    {
        _isUnqueuing = true;
        var first = _generatorQueu.FirstOrDefault();
        if (first == null)
        {
            _isUnqueuing = false;
            GC.Collect();
            return;
        }

        var document = first.Item2;
        var notification = first.Item1;
        try
        {
            document.GeneratePdf(notification.FilePath);
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(async () => await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync());
            Messenger.Send(new EditedMessage(new ChangedItem { Id = notification.FileId, Where = "FailedToGenerate" }));
        }
        notification.CurrStep++;
        notification.StatusOfFile = FileStatus.Ready;
        notification.Description!.Description += $"\nДата и время создания файла: {DateTime.Now}";
        Messenger.Send(new EditedMessage(new ChangedItem
            { Id = notification.FileId, MainName = "", Where = "FileReady" }));
        _generatorQueu.Remove(first);
        
        await UnQueu();
    }

    public async Task GenerateInvoiceExcel(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
        try
        {
            await Task.Run(async () => await _excelInvoice.CreateExcel(parts, mainGroup, fileId, notification));
            Messenger.Send(new EditedMessage(new ChangedItem
                { Id = notification.FileId, MainName = "", Where = "FileReady" }));
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(async () => await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync());
            Messenger.Send(new EditedMessage(new ChangedItem { Id = notification.FileId, Where = "FailedToGenerate" }));
        }
    }
    
    public void GenerateInvoice(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
       var document = Task.Run(() => _invoice.CreateDocument(parts, mainGroup, fileId, notification)).Result;
       _generatorQueu.Add(document);
       TryUnqueu();
    }

    public void GenerateInvoiceForPeriod(IEnumerable<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>> tuples, DateTime start, DateTime end, int fileId, NotificationModel notification)
    {
        var document = Task.Run(() => _invoiceForPeriod.CreateInvoice(tuples, start, end, fileId, notification)).Result;
        _generatorQueu.Add(document);
        TryUnqueu();
    }

    public void GenerateInvoiceForPeriodMinimal(IEnumerable<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>> tuples, DateTime startDate, DateTime endDate, int fileId, NotificationModel notification)
    {
        var document = Task.Run(() => _invoiceForPeriodMinimal.CreateInvoice(tuples, startDate, endDate, fileId, notification)).Result;
        _generatorQueu.Add(document);
        TryUnqueu();
    }
}