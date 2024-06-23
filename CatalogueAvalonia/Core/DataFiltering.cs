using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CatalogueAvalonia.Core;

public class DataFiltering
{
    private static readonly Regex reg = new(@"[^a-zА-Яа-яA-Z0-9_]+");
    private static readonly Regex regSlashes = new(@"([/\\])+|([/\\])\2*");

    public static async IAsyncEnumerable<CatalogueModel> FilterByMainName(List<CatalogueModel> catalogueModels,
        string objectToFind, [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 0; i < catalogueModels.Count(); i++)
            if (await CheckIfContainsName(catalogueModels[i].Name, objectToFind))
                yield return catalogueModels[i];
    }

    public static async IAsyncEnumerable<CatalogueModel> FilterByUniValue(List<CatalogueModel> catalogueModels,
        string objectToFind, [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 0; i < catalogueModels.Count(); i++)
            if (catalogueModels[i].Children != null)
                for (var k = 0; k < catalogueModels[i].Children.Count; k++)
                    if (await CheckIfContainsUniValue(catalogueModels[i].Children[k], objectToFind))
                    {
                        yield return catalogueModels[i];
                        break;
                    }
    }

    public static async IAsyncEnumerable<ProducerModel> FilterProducer(IEnumerable<ProducerModel> producerModels,
        string objectToFind)
    {
        foreach (var producerModel in producerModels)
            if (await CheckIfContainsName(producerModel.ProducerName, objectToFind))
                yield return producerModel;
    }

    public static async IAsyncEnumerable<AgentModel> FilterAgents(IEnumerable<AgentModel> agentModels,
        string objectToFind)
    {
        foreach (var agentModel in agentModels)
            if (await CheckIfContainsName(agentModel.Name, objectToFind))
                yield return agentModel;
    }

    public static async Task<List<string>> FilterSlashes(string textWithSlashes)
    {
        var addedItems = new List<string>();
        var result = "";
        var models = await ReplaceSlashesAsync(textWithSlashes, "/");

        for (var i = 0; i < models.Length; i++)
            if (models[i] == '/' && !string.IsNullOrEmpty(result))
            {
                if (!addedItems.Contains(result))
                {
                    addedItems.Add(result);
                    result = string.Empty;
                }
                else
                {
                    result = string.Empty;
                }
            }
            else
            {
                result += models[i];
            }

        return addedItems;
    }

    private static Task<string> ReplaceSlashesAsync(string text, string replaceWith)
    {
        return Task.FromResult(regSlashes.Replace(text, replaceWith));
    }

    private static Task<bool> CheckIfContainsUniValue(CatalogueModel catalogueModel, string ObjectToFind)
    {
        return Task.FromResult(reg.Replace(catalogueModel.UniValue, "").ToLower()
            .Contains(reg.Replace(ObjectToFind, "").ToLower()));
    }

    private static Task<bool> CheckIfContainsName(string name, string ObjectToFind)
    {
        return Task.FromResult(reg.Replace(name, "").ToLower().Contains(reg.Replace(ObjectToFind, "").ToLower()));
    }

    public static Task<JArray?> FromJsonToArray(string jsonData)
    {
        return Task.FromResult((JArray?)JsonConvert.DeserializeObject(jsonData));
    }
}