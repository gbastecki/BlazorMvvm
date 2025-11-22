using System;

namespace BlazorMvvm
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BlazorCommandAttribute : Attribute
    {
        public string? CanExecute { get; set; }
        public object? AllowConcurrentExecutions { get; set; }

        public BlazorCommandAttribute(string? canExecute = null, object? allowConcurrentExecutions = null)
        {
            CanExecute = canExecute;
            AllowConcurrentExecutions = allowConcurrentExecutions;
        }
    }
}
