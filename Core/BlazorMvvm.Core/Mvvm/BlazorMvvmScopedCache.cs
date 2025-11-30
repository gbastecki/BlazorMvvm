using System;
using System.Collections.Generic;

namespace BlazorMvvm
{
    public class BlazorMvvmScopedCache
    {
        public Dictionary<Type, object> Cache { get; } = new();
    }
}
