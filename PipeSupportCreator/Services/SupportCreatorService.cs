using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace PipeSupportCreator.Services;

public class SupportCreatorService
{
    private const string FamilyName = "Placeholder";
    private const string FamilySymbolName = "PlaceholderType";
    private const string ParameterName = "Height";
    
    private Document _document = null!;
    private View3D _temporaryView = null!;
    
    public void CreateSupports(Document document, Pipe pipe)
    {
        _document = document;
        
        var pipeCurve = ((LocationCurve)pipe.Location).Curve;
        var startPoint = pipeCurve.GetEndPoint(0);
        var endPoint = pipeCurve.GetEndPoint(1);
        
        var symbol = GetPlaceholderFamilySymbol();
        if (symbol is null)
        {
            TaskDialog.Show("Error", "Could not find placeholder family symbol.");
            return;
        }

        using var transaction = new Transaction(_document, "Place Pipe Supports");
        transaction.Start();
        
        _temporaryView = CreateTemporaryView();
        ProcessPipeEnd(startPoint, symbol);
        ProcessPipeEnd(endPoint, symbol);
        _document.Delete(_temporaryView.Id);
        
        transaction.Commit();
    }

    private View3D CreateTemporaryView()
    {
        var randomView = _document.EnumerateInstances<View3D>().First(view3D => !view3D.IsTemplate);
        var view = randomView.Duplicate(ViewDuplicateOption.Duplicate).ToElement<View3D>(randomView.Document);
        view.ViewTemplateId = ElementId.InvalidElementId;
        view.DetailLevel = ViewDetailLevel.Fine;
        view.CropBoxActive = false;
        
        return view;
    }

    private FamilySymbol? GetPlaceholderFamilySymbol()
    {
        return _document.EnumerateInstances<Family>()
            .First(family => family.Name == FamilyName)
            .GetFamilySymbolIds()
            .FirstOrDefault(id => id.ToElement(_document)!.Name == FamilySymbolName)
            .ToElement<FamilySymbol>(_document);
    }

    private void ProcessPipeEnd(XYZ point, FamilySymbol symbol)
    {
        var floorId = FindNearestFloorIdAbove(point, _temporaryView, out var isFloorLinked);
        var floorDocument = isFloorLinked ? FindLinkedDocumentWithElement<Floor>(floorId) : _document;
        var floor = floorId.ToElement<Floor>(floorDocument);
        
        var instance = PlaceFamilyOnEndpoint(point, symbol);
        ExtendHeightToFloor(instance, point, floor);
    }

    private ElementId FindNearestFloorIdAbove(XYZ point, View3D temporaryView, out bool isLinkedElement)
    {
        var referenceIntersector = new ReferenceIntersector(temporaryView)
        {
            FindReferencesInRevitLinks = true,
            TargetType = FindReferenceTarget.Element
        };
        referenceIntersector.SetFilter(new ElementClassFilter(typeof(Floor)));

        var reference = referenceIntersector
            .FindNearest(point, XYZ.BasisZ)
            .GetReference();

        isLinkedElement = reference.LinkedElementId != ElementId.InvalidElementId;
        return isLinkedElement ? reference.LinkedElementId : reference.ElementId;
    }

    private Document? FindLinkedDocumentWithElement<T>(ElementId elementId) where T : Element
    {
        // returns the first linked document that has as ElementId which belongs to (T)Element
        return _document.EnumerateInstances<RevitLinkInstance>()
            .Select(link => link.GetLinkDocument())
            .Where(doc => elementId.ToElement(doc) is not null)
            .FirstOrDefault(doc => elementId.ToElement(doc) is T);
    }

    private FamilyInstance PlaceFamilyOnEndpoint(XYZ point, FamilySymbol symbol)
    {
        return _document.Create.NewFamilyInstance(point, symbol, StructuralType.NonStructural);
    }

    private void ExtendHeightToFloor(FamilyInstance instance, XYZ instanceLocation, Floor floor)
    {
        instance.FindParameter(ParameterName)?.Set(floor.get_BoundingBox(null).Min.Z - instanceLocation.Z);
    }
}