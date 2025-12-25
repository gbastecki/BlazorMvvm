using System;

namespace BlazorMvvm
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BlazorCommandAttribute : Attribute
    {
        public string? CanExecute { get; set; }
        public object? AllowConcurrentExecutions { get; set; }
        public string? OnIsExecutingChangedCallback { get; set; }
        public bool AutoRefreshOnIsExecutingChanged { get; set; }

        public BlazorCommandAttribute(string? canExecute = null, object? allowConcurrentExecutions = null, string? onIsExecutingChangedCallback = null, bool autoRefreshOnIsExecutingChanged = false)
        {
            CanExecute = canExecute;
            AllowConcurrentExecutions = allowConcurrentExecutions;
            OnIsExecutingChangedCallback = onIsExecutingChangedCallback;
            AutoRefreshOnIsExecutingChanged = autoRefreshOnIsExecutingChanged;
        }
    }
}