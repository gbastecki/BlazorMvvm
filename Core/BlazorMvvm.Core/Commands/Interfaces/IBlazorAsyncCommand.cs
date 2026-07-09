using System.Threading.Tasks;

namespace BlazorMvvm;
public interface IBlazorAsyncCommand
{
    public delegate void IsExecutingChanged(bool isExecuting);
    public event IsExecutingChanged? OnIsExecutingChanged;
    public bool AllowConcurrentExecutions { get; set; }
    public bool ContinueOnCapturedContext { get; set; }
    public bool IsExecuting { get; }
    public Task<bool> CanExecute();
    public Task ExecuteAsync();
    public void Execute();
}