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
        public bool ContinueOnCapturedContext { get; set; }

        public BlazorCommandAttribute(string? canExecute = null, object? allowConcurrentExecutions = null, string? onIsExecutingChangedCallback = null, bool autoRefreshOnIsExecutingChanged = false, bool continueOnCapturedContext = true)
        {
            CanExecute = canExecute;
            AllowConcurrentExecutions = allowConcurrentExecutions;
            OnIsExecutingChangedCallback = onIsExecutingChangedCallback;
            AutoRefreshOnIsExecutingChanged = autoRefreshOnIsExecutingChanged;
            ContinueOnCapturedContext = continueOnCapturedContext;
        }
    }
}