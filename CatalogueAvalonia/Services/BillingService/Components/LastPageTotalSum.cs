using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CatalogueAvalonia.Services.BillingService.Components;

public class LastPageTotalSum : IDynamicComponent<SumCountState>
{
    public SumCountState State { get; set; }

    public LastPageTotalSum(int count, decimal sum)
    {
        State = new() { TotalSum = sum, TotalCont = count };
    }
    
    public DynamicComponentComposeResult Compose(DynamicContext context)
    {
        var content = context.CreateElement(element =>
        {
            if (context.PageNumber == context.TotalPages)
            {
                element.AlignRight().AlignBottom()
                    .Text($"Общая сумма:{State.TotalSum}     Общее колчество:{State.TotalCont}").FontSize(10);
            }
        });
        
        return new DynamicComponentComposeResult()
        {
            Content = content,
            HasMoreContent = false
        };
    }
}
