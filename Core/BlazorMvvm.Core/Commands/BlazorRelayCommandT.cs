using System;

namespace BlazorMvvm;
public class BlazorRelayCommand<T> : IBlazorRelayCommand<T>
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public BlazorRelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }

    public bool CanExecute(T arg)
    {
        return this._canExecute == null || this._canExecute(arg);
    }

    public void Execute(T arg)
    {
        if (!CanExecute(arg)) return;
        this._execute(arg);
    }
}