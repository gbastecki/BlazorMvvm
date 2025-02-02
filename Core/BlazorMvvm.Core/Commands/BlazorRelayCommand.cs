using System;

namespace BlazorMvvm;
public class BlazorRelayCommand : IBlazorRelayCommand
{
    private readonly Action<object[]?> _execute;
    private readonly Func<object[]?, bool>? _canExecute;

    public BlazorRelayCommand(Action<object[]?> execute, Func<object[]?, bool>? canExecute = null)
    {
        this._execute = execute;
        this._canExecute = canExecute;
    }

    public bool CanExecute(params object[]? args)
    {
        return this._canExecute == null || this._canExecute(args);
    }

    public void Execute(params object[]? args)
    {
        if (!CanExecute(args)) return;
        this._execute(args);
    }
}