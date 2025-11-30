using System;
using System.Collections.Concurrent;

namespace BlazorMvvm
{
    public static class ViewModelRegistry
    {
        private static readonly ConcurrentDictionary<Type, (Func<IServiceProvider, IBlazorViewModel>? Factory, ViewModelLifetime Lifetime)> _registry = new();

        public static void Register(Type viewModelType, Func<IServiceProvider, IBlazorViewModel>? factory, ViewModelLifetime lifetime)
        {
            _registry[viewModelType] = (factory, lifetime);
        }

        public static void Register(Type viewModelType, ViewModelLifetime lifetime)
        {
            Register(viewModelType, null, lifetime);
        }

        public static (Func<IServiceProvider, IBlazorViewModel>? Factory, ViewModelLifetime Lifetime) GetViewModelInfo(Type viewModelType)
        {
            if (_registry.TryGetValue(viewModelType, out var info))
            {
                return info;
            }
            return (null, ViewModelLifetime.Transient);
        }
    }
}
