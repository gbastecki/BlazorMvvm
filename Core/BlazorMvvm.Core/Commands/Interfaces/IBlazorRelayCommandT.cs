namespace BlazorMvvm;
public interface IBlazorRelayCommand<T>
{
    public bool CanExecute(T arg);
    public void Execute(T arg);
}