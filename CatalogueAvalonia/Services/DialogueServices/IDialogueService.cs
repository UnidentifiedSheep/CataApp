using System.Threading.Tasks;
using Avalonia.Controls;

namespace CatalogueAvalonia.Services.DialogueServices;

public interface IDialogueService
{
    Task OpenDialogue(Window window, ViewModelBase viewModel, Window parent);
    void OpenWindow(Window window, ViewModelBase viewModel);
}