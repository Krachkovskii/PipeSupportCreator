using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using PipeSupportCreator.Services;

namespace PipeSupportCreator.Commands;

/// <summary>
///     External command entry point invoked from the Revit interface
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class SupportCreatorCommand : ExternalCommand
{
    public override void Execute()
    {
        var pipe = new PipeSelectionService().PromptPipeSelection(UiDocument);
        if (pipe is null) return;

        new SupportCreatorService().ExtendPipe(Document, pipe);
    }
}