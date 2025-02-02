namespace BlazorMvvm;
public interface IBlazorRelayCommand
{
    public bool CanExecute(params object[]? args);
    public void Execute(params object[]? args);
}