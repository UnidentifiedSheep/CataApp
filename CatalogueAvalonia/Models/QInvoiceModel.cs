using System;
using System.Collections.Generic;

namespace CatalogueAvalonia.Models;

public class QInvoiceModel
{
    public int FileId;
    public DateTime? StartDate;
    public DateTime? EndDate;
    public List<ProdajaModel> ProdajaModels = new();
    public NotificationModel? NotificationModel;
    public InvoiceType InvoiceType;
}

public enum InvoiceType
{
    SingleInvoice = 0,
    InvoiceForPeriod = 1,
    InvoiceForPeriodMinimal = 2,
    SingleInvoiceExcel = 3,
}