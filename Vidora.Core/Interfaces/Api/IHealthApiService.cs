using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace Vidora.Core.Interfaces.Api;

public interface IHealthApiService
{
    Task CheckHealthAsync();
}
