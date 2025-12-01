using System;
using System.Collections.Concurrent;

namespace BlazorMvvm
{
    public class BlazorMvvmViewModelFactory : IBlazorMvvmViewModelFactory
    {
        private readonly ConcurrentDictionary<Type, object> _singletonCache = new();

        public IBlazorViewModel? GetViewModel(Type viewModelType, IServiceProvider serviceProvider, BlazorMvvmScopedCache scopedCache)
        {
            var (factory, lifetime) = ViewModelRegistry.GetViewModelInfo(viewModelType);
            object? vm;

            if (lifetime == ViewModelLifetime.Singleton)
            {
                vm = CreateViewModel(serviceProvider, factory);
                if (vm == null) return null;
                return (IBlazorViewModel)_singletonCache.GetOrAdd(viewModelType, t => vm);
            }

            if (lifetime == ViewModelLifetime.Scoped)
            {
                if (scopedCache.Cache.TryGetValue(viewModelType, out var cachedVm))
                {
                    return (IBlazorViewModel)cachedVm;
                }

                vm = CreateViewModel(serviceProvider, factory);
                if (vm == null) return null;
                scopedCache.Cache[viewModelType] = vm;
                return (IBlazorViewModel)vm;
            }

            vm = CreateViewModel(serviceProvider, factory);
            if (vm == null) return null;
            return (IBlazorViewModel)vm;
        }

        private static object? CreateViewModel(IServiceProvider serviceProvider, Func<IServiceProvider, IBlazorViewModel>? factory)
        {
            if (factory != null)
            {
                return factory(serviceProvider);
            }
            return null;
        }
    }
}
