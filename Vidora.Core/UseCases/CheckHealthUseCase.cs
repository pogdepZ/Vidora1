using System;
using System.Threading.Tasks;
using Vidora.Core.Exceptions;
using Vidora.Core.Interfaces.Api;
namespace Vidora.Core.UseCases;

public class CheckHealthUseCase
{
    private readonly IHealthApiService _healthApiService;
    public CheckHealthUseCase(IHealthApiService healthApiService)
    {
        _healthApiService = healthApiService;
    }

    public Task ExecuteAsync()
    {
        {
            try
            {
                return ExecuteAsyncInternal();
            }
            catch (UnauthorizationException)
            {
                throw;
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException(ex.Message);
            }
        }
    }

    private Task ExecuteAsyncInternal()
    {
        return _healthApiService.CheckHealthAsync();
    }
}
