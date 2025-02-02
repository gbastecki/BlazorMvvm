using System.Threading.Tasks;

namespace BlazorMvvm;
public interface IBlazorAsyncRelayCommand<T>
{
    public delegate void IsExecutingChanged(bool isExecuting);
    public event IsExecutingChanged? OnIsExecutingChanged;
    public bool AllowConcurrentExecutions { get; set; }
    public bool IsExecuting { get; }
    public Task<bool> CanExecute(T arg);
    public void Execute(T arg);
}