using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public class BlazorAsyncRelayCommand : IBlazorAsyncRelayCommand
{
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);

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
        if (this._canExecute != null && !await this._canExecute(args)) return;
        bool shouldExecute = AllowConcurrentExecutions || await _executionSemaphore.WaitAsync(0);
        if (!shouldExecute) return;

        try
        {
            IsExecuting = true;
            await this._execute(args);
        }
        finally
        {
            IsExecuting = false;
            if (!AllowConcurrentExecutions) _executionSemaphore.Release();
        }
    }
}