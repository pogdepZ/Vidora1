using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}