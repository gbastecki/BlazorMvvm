using System.Threading.Tasks;

namespace BlazorMvvm;
public interface IBlazorAsyncRelayCommand
{
    public delegate void IsExecutingChanged(bool isExecuting);
    public event IsExecutingChanged? OnIsExecutingChanged;
    public bool AllowConcurrentExecutions { get; set; }
    public bool ContinueOnCapturedContext { get; set; }
    public bool IsExecuting { get; }
    public Task<bool> CanExecute(params object[]? args);
    public Task ExecuteAsync(params object[]? args);
    public void Execute(params object[]? args);
}