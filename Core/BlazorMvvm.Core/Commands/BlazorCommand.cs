using System;

namespace BlazorMvvm;
public class BlazorCommand : IBlazorCommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public BlazorCommand(Action execute, Func<bool>? canExecute = null)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }

    public bool CanExecute()
    {
        return this._canExecute == null || this._canExecute();
    }

    public void Execute()
    {
        if (!CanExecute()) return;
        this._execute();
    }
}