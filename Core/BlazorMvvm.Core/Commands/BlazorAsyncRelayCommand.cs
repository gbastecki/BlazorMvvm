using System;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncRelayCommand : IBlazorAsyncRelayCommand
{
    private readonly Func<object[]?, Task> _execute;
    private readonly Func<object[]?, Task<bool>>? _canExecute;

    public event IBlazorAsyncRelayCommand.IsExecutingChanged? OnIsExecutingChanged;
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

    public BlazorAsyncRelayCommand(Func<object[]?, Task> execute, Func<object[]?, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute;
        this._canExecute = canExecute;
        this.AllowConcurrentExecutions = allowConcurrentExecutions;
    }

    public async Task<bool> CanExecute(params object[]? args)
    {
        if (!AllowConcurrentExecutions && IsExecuting) return false;
        return this._canExecute == null || await this._canExecute(args);
    }

    public async void Execute(params object[]? args)
    {
        try
        {
            if (!await CanExecute(args)) return;
            IsExecuting = true;
            await this._execute(args);
        }
        finally
        {
            IsExecuting = false;
        }
    }
}