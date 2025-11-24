using System;

namespace Vidora.Presentation.Gui.Contracts.Services;


public interface IPageService
{
    Type GetPageType(string key);

    Type GetPageType<TViewModel>() where TViewModel : class;
}