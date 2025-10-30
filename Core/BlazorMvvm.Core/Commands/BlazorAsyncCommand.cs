using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncCommand : IBlazorAsyncCommand
{
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);

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

    public BlazorAsyncCommand(Func<Task> execute, Func<Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
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
        if (this._canExecute != null && !await this._canExecute()) return;
        bool shouldExecute = AllowConcurrentExecutions || await _executionSemaphore.WaitAsync(0);
        if (!shouldExecute) return;

        try
        {
            IsExecuting = true;
            await this._execute();
        }
        finally
        {
            IsExecuting = false;
            if (!AllowConcurrentExecutions) _executionSemaphore.Release();
        }
    }
}