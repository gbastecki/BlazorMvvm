using System;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncRelayCommand<T> : IBlazorAsyncRelayCommand<T>
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, Task<bool>>? _canExecute;

    public event IBlazorAsyncRelayCommand<T>.IsExecutingChanged? OnIsExecutingChanged;
    public bool AllowConcurrentExecutions { get; set; }
#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _executingLock = new();
    private bool _isExecuting;
    public bool IsExecuting
    {
        get
        {
            using (_executingLock.EnterScope())
            {
                return _isExecuting;
            }
        }
        private set
        {
            using (_executingLock.EnterScope())
            {
                _isExecuting = value;
                OnIsExecutingChanged?.Invoke(value);
            }
        }
    }
#else
    private readonly object _executingLock = new();
    private bool _isExecuting;
    public bool IsExecuting
    {
        get
        {
            lock (_executingLock)
            {
                return _isExecuting;
            }
        }
        private set
        {
            lock (_executingLock)
            {
                _isExecuting = value;
                OnIsExecutingChanged?.Invoke(value);
            }
        }
    }
#endif

    public BlazorAsyncRelayCommand(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute;
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
        try
        {
            if (!await CanExecute(arg)) return;
            IsExecuting = true;
            await this._execute(arg);
        }
        finally
        {
            IsExecuting = false;
        }
    }
}