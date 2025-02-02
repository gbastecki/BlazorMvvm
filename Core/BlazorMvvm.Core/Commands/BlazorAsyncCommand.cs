using System;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncCommand : IBlazorAsyncCommand
{
    private readonly Func<Task> _execute;
    private readonly Func<Task<bool>>? _canExecute;

    public event IBlazorAsyncCommand.IsExecutingChanged? OnIsExecutingChanged;
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

    public BlazorAsyncCommand(Func<Task> execute, Func<Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute;
        this._canExecute = canExecute;
        this.AllowConcurrentExecutions = allowConcurrentExecutions;
    }

    public async Task<bool> CanExecute()
    {
        if (!AllowConcurrentExecutions && IsExecuting) return false;
        return this._canExecute == null || await this._canExecute();
    }

    public async void Execute()
    {
        try
        {
            if (!await CanExecute()) return;
            IsExecuting = true;
            await this._execute();
        }
        finally
        {
            IsExecuting = false;
        }
    }
}