namespace BlazorMvvm;
public interface IBlazorCommand
{
    public bool CanExecute();
    public void Execute();
}