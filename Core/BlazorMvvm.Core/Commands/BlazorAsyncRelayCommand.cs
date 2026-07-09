using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncRelayCommand : IBlazorAsyncRelayCommand
{
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);
    private int _runningTasksCount;

    private readonly Func<object[]?, Task> _execute;
    private readonly Func<object[]?, Task<bool>>? _canExecute;

    public event IBlazorAsyncRelayCommand.IsExecutingChanged? OnIsExecutingChanged;
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

    public BlazorAsyncRelayCommand(Func<object[]?, Task> execute, Func<object[]?, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
        this.AllowConcurrentExecutions = allowConcurrentExecutions;
    }

    public async Task<bool> CanExecute(params object[]? args)
    {
        if (!AllowConcurrentExecutions && IsExecuting) return false;
        return this._canExecute == null || await this._canExecute(args).ConfigureAwait(ContinueOnCapturedContext);
    }

    public async Task ExecuteAsync(params object[]? args)
    {
        if (this._canExecute != null && !await this._canExecute(args).ConfigureAwait(ContinueOnCapturedContext)) return;
        bool shouldExecute = AllowConcurrentExecutions || await _executionSemaphore.WaitAsync(0).ConfigureAwait(ContinueOnCapturedContext);
        if (!shouldExecute) return;

        try
        {
            if (Interlocked.Increment(ref _runningTasksCount) == 1)
            {
                IsExecuting = true;
            }
            await this._execute(args).ConfigureAwait(ContinueOnCapturedContext);
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

    public void Execute(params object[]? args)
    {
        _ = ExecuteAsync(args);
    }
}