using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlazorMvvm;
public interface IBlazorViewModel
{
    public delegate void TriggerRefresh(string? propertyName);
    public delegate Task TriggerRefreshAsync(string? propertyName);

    public event TriggerRefresh? OnTriggerRefresh;
    public event TriggerRefreshAsync? OnTriggerRefreshAsync;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null);
    public Task OnPropertyChangedAsync([CallerMemberName] string? propertyName = null, bool continueOnCapturedContext = true);

    public void Set<TValue>(ref TValue property, TValue value, [CallerMemberName] string? propertyName = null);

    public void DebounceRefresh(int delayMilliseconds, [CallerMemberName] string? propertyName = null);
}
