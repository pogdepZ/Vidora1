using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    public DefaultActivationHandler()
    {
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return false;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        await Task.CompletedTask;
    }
}