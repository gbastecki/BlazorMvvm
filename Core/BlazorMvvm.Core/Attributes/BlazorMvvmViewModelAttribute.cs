using System;

namespace BlazorMvvm
{
    public enum ViewModelLifetime
    {
        Transient,
        Scoped,
        Singleton
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BlazorMvvmViewModelAttribute : Attribute
    {
        public ViewModelLifetime Lifetime { get; }

        public BlazorMvvmViewModelAttribute(ViewModelLifetime lifetime = ViewModelLifetime.Transient)
        {
            Lifetime = lifetime;
        }
    }
}
