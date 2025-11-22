using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncRelayCommand<T> : IBlazorAsyncRelayCommand<T>
{
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);

    private readonly Func<T, Task> _execute;
    private readonly Func<T, Task<bool>>? _canExecute;

    public event IBlazorAsyncRelayCommand<T>.IsExecutingChanged? OnIsExecutingChanged;
    public bool AllowConcurrentExecutions { get; set; }

    private volatile bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting == value) return;
            _isExecuting = value;
            OnIsExecutingChanged?.Invoke(value);
        }
    }

    public BlazorAsyncRelayCommand(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
        this.AllowConcurrentExecutions = allowConcurrentExecutions;
    }

    public async Task<bool> CanExecute(T arg)
    {
        if (!AllowConcurrentExecutions && IsExecuting) return false;
        return this._canExecute == null || await this._canExecute(arg);
    }

    public async void Execute(T arg)
    {
        if (this._canExecute != null && !await this._canExecute(arg)) return;
        bool shouldExecute = AllowConcurrentExecutions || await _executionSemaphore.WaitAsync(0);
        if (!shouldExecute) return;

        try
        {
            IsExecuting = true;
            await this._execute(arg);
        }
        finally
        {
            IsExecuting = false;
            if (!AllowConcurrentExecutions) _executionSemaphore.Release();
        }
    }
}