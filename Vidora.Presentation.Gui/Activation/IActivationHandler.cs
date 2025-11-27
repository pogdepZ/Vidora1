using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}