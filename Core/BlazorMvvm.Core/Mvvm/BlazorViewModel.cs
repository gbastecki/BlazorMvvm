using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlazorMvvm;
public abstract class BlazorViewModel : IBlazorViewModel
{
    public event IBlazorViewModel.TriggerRefresh? OnTriggerRefresh;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnTriggerRefresh?.Invoke(propertyName);
    }
    public async Task OnPropertyChangedAsync([CallerMemberName] string? propertyName = null)
    {
        await Task.Run(() => OnPropertyChanged(propertyName));
    }

    public void Set<TValue>(ref TValue property, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(property, value)) return;
        property = value;
        OnPropertyChanged(propertyName);
    }
}