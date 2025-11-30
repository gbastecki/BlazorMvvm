using System;

namespace BlazorMvvm
{
    public interface IBlazorMvvmViewModelFactory
    {
        IBlazorViewModel GetViewModel(Type viewModelType, IServiceProvider serviceProvider, BlazorMvvmScopedCache scopedCache);
    }
}
