using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncCommand : IBlazorAsyncCommand
{
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);
    private int _runningTasksCount;

    private readonly Func<Task> _execute;
    private readonly Func<Task<bool>>? _canExecute;

    public event IBlazorAsyncCommand.IsExecutingChanged? OnIsExecutingChanged;
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

    public bool ContinueOnCapturedContext { get; set; } = true;

    public BlazorAsyncCommand(Func<Task> execute, Func<Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
        this.AllowConcurrentExecutions = allowConcurrentExecutions;
    }

    public async Task<bool> CanExecute()
    {
        if (!AllowConcurrentExecutions && IsExecuting) return false;
        return this._canExecute == null || await this._canExecute().ConfigureAwait(ContinueOnCapturedContext);
    }

    public async Task ExecuteAsync()
    {
        if (this._canExecute != null && !await this._canExecute().ConfigureAwait(ContinueOnCapturedContext)) return;
        bool shouldExecute = AllowConcurrentExecutions || await _executionSemaphore.WaitAsync(0).ConfigureAwait(ContinueOnCapturedContext);
        if (!shouldExecute) return;

        try
        {
            if (Interlocked.Increment(ref _runningTasksCount) == 1)
            {
                IsExecuting = true;
            }
            await this._execute().ConfigureAwait(ContinueOnCapturedContext);
        }
        finally
        {
            if (Interlocked.Decrement(ref _runningTasksCount) == 0)
            {
                IsExecuting = false;
            }
            if (!AllowConcurrentExecutions) _executionSemaphore.Release();
        }
    }

    public void Execute()
    {
        _ = ExecuteAsync();
    }
}