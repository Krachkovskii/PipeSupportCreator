using Nice3point.Revit.Toolkit.External;
using PipeSupportCreator.Commands;

namespace PipeSupportCreator;

/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreateRibbonPanel("PipeSupportCreator");

        panel.AddPushButton<SupportCreatorCommand>("Place Pipe\nSupports")
            .SetImage("/PipeSupportCreator;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/PipeSupportCreator;component/Resources/Icons/RibbonIcon32.png");
    }
}