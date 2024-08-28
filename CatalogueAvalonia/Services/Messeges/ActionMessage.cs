using System.Collections;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DataBase.Data;

namespace CatalogueAvalonia.Services.Messeges;

public class ActionMessage : ValueChangedMessage<ActionM>
{
    public ActionMessage(ActionM action) : base(action)
    {
    }
    
}
public class ActionM
{
    public ActionM( string value,IEnumerable<AgentBalance>? balances = null)
    {
        Balances = balances;
        Value = value;
    }

    public string Value { get; private set; }
    public IEnumerable<AgentBalance>? Balances { get; private set; }
}