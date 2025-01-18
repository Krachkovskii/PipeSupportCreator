using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PipeSupportCreator.Utils;

namespace PipeSupportCreator.Services;

public class PipeSelectionService
{
    public Pipe? PromptPipeSelection(UIDocument uiDocument)
    {
        var firstSelectedPipe = uiDocument.Selection
            .GetElementIds()
            .FirstOrDefault(id => id.ToElement<Pipe>(uiDocument.Document) is not null);
        if (firstSelectedPipe is not null) return firstSelectedPipe.ToElement<Pipe>(uiDocument.Document);
            
        var picked = uiDocument.Selection
            .PickObject(ObjectType.Element, new PipeSelectionFilter(), "Select pipe to place a family");
        return picked?.ElementId.ToElement<Pipe>(uiDocument.Document);
    }
}