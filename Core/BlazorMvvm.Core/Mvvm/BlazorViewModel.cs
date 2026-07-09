using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMvvm;
public abstract class BlazorViewModel : IBlazorViewModel
{
    private readonly Dictionary<string, CancellationTokenSource> _debounceTokens = new();
    private readonly object _debounceLock = new();

    public event IBlazorViewModel.TriggerRefresh? OnTriggerRefresh;
    public event IBlazorViewModel.TriggerRefreshAsync? OnTriggerRefreshAsync;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnTriggerRefresh?.Invoke(propertyName);

        var asyncHandlers = OnTriggerRefreshAsync;
        if (asyncHandlers != null)
        {
            foreach (IBlazorViewModel.TriggerRefreshAsync handler in asyncHandlers.GetInvocationList())
            {
                _ = handler(propertyName);
            }
        }
    }

    public async Task OnPropertyChangedAsync([CallerMemberName] string? propertyName = null, bool continueOnCapturedContext = true)
    {
        OnTriggerRefresh?.Invoke(propertyName);

        var asyncHandlers = OnTriggerRefreshAsync;
        if (asyncHandlers != null)
        {
            List<Task> tasks = new();
            foreach (IBlazorViewModel.TriggerRefreshAsync handler in asyncHandlers.GetInvocationList())
            {
                var task = handler(propertyName);
                if (task != null)
                {
                    tasks.Add(task);
                }
            }
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext);
            }
        }
    }

    public void Set<TValue>(ref TValue property, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(property, value)) return;
        property = value;
        OnPropertyChanged(propertyName);
    }

    public void DebounceRefresh(int delayMilliseconds, [CallerMemberName] string? propertyName = null)
    {
        string key = propertyName ?? string.Empty;

        lock (_debounceLock)
        {
            // Cancel existing timer for this specific property
            if (_debounceTokens.TryGetValue(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            // If full refresh (null/empty key), cancel all individual property timers
            if (key == string.Empty)
            {
                foreach (var kvp in _debounceTokens)
                {
                    if (kvp.Key != string.Empty)
                    {
                        kvp.Value.Cancel();
                        kvp.Value.Dispose();
                    }
                }
                _debounceTokens.Clear();
            }

            CancellationTokenSource newCts = new();
            _debounceTokens[key] = newCts;

            _ = StartDebounceTimerAsync(delayMilliseconds, propertyName, newCts.Token);
        }
    }

    private async Task StartDebounceTimerAsync(int delayMilliseconds, string? propertyName, CancellationToken token)
    {
        try
        {
            await Task.Delay(delayMilliseconds, token);

            lock (_debounceLock)
            {
                string key = propertyName ?? string.Empty;
                if (_debounceTokens.TryGetValue(key, out var cts) && cts.Token == token)
                {
                    _debounceTokens.Remove(key);
                    cts.Dispose();
                }
                else
                {
                    return;
                }
            }

            OnPropertyChanged(propertyName);
        }
        catch (TaskCanceledException)
        {
            // Discard silently
        }
    }

    public void CancelAllDebounces()
    {
        lock (_debounceLock)
        {
            foreach (var cts in _debounceTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _debounceTokens.Clear();
        }
    }
}