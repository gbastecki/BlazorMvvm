using System;
using System.Collections.Concurrent;

namespace BlazorMvvm
{
    public class BlazorMvvmViewModelFactory : IBlazorMvvmViewModelFactory
    {
        private readonly ConcurrentDictionary<Type, object> _singletonCache = new();

        public IBlazorViewModel GetViewModel(Type viewModelType, IServiceProvider serviceProvider, BlazorMvvmScopedCache scopedCache)
        {
            var (factory, lifetime) = ViewModelRegistry.GetViewModelInfo(viewModelType);

            if (lifetime == ViewModelLifetime.Singleton)
            {
                return (IBlazorViewModel)_singletonCache.GetOrAdd(viewModelType, t => CreateViewModel(t, serviceProvider, factory));
            }

            if (lifetime == ViewModelLifetime.Scoped)
            {
                if (scopedCache.Cache.TryGetValue(viewModelType, out var cachedVm))
                {
                    return (IBlazorViewModel)cachedVm;
                }

                var vm = CreateViewModel(viewModelType, serviceProvider, factory);
                scopedCache.Cache[viewModelType] = vm;
                return (IBlazorViewModel)vm;
            }

            return (IBlazorViewModel)CreateViewModel(viewModelType, serviceProvider, factory);
        }

        private static object CreateViewModel(Type viewModelType, IServiceProvider serviceProvider, Func<IServiceProvider, IBlazorViewModel>? factory)
        {
            if (factory != null)
            {
                return factory(serviceProvider);
            }
            throw new Exception($"No factory registered for ViewModel type {viewModelType.FullName}.");
        }
    }
}
