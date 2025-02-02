using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlazorMvvm;
public interface IBlazorViewModel
{
    public delegate void TriggerRefresh(string? propertyName);
    public event TriggerRefresh? OnTriggerRefresh;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null);
    public Task OnPropertyChangedAsync([CallerMemberName] string? propertyName = null);

    public void Set<TValue>(ref TValue property, TValue value, [CallerMemberName] string? propertyName = null);
}
