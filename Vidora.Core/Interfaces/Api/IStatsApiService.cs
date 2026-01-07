using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IStatsApiService
{
    Task<Result<DashboardResult>> GetDashboardAsync();
}
