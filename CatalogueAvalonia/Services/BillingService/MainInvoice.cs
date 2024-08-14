using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
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
        Task.Run(() => UnQueu());
    }

    private bool _isUnqueuing = false;
    private void UnQueu()
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
        document.GeneratePdf(notification.FilePath);
        notification.CurrStep++;
        notification.StatusOfFile = FileStatus.Ready;
        notification.Description!.Description += $"\nДата и время создания файла: {DateTime.Now}";
        Messenger.Send(new EditedMessage(new ChangedItem
            { Id = notification.FileId, MainName = "", Where = "FileReady" }));
        _generatorQueu.Remove(first);
        
        UnQueu();
    }

    public void GenerateInvoiceExcel(IEnumerable<ProdajaAltModel> parts, ProdajaModel mainGroup, int fileId, NotificationModel notification)
    {
        Task.Run(() => _excelInvoice.CreateExcel(parts, mainGroup, fileId, notification));
        Messenger.Send(new EditedMessage(new ChangedItem
            { Id = notification.FileId, MainName = "", Where = "FileReady" }));
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